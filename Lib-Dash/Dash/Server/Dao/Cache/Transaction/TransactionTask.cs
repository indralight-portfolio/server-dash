#if Common_Server
using NLog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Dash.Server.Dao.Cache.Transaction
{
    public class TransactionTask
    {
        private readonly AbstractDao _dao;
        private readonly DaoCache _daoCache;
        private List<string> _queries = new List<string>();
        private List<List<KeyValuePair<string, object>>> _params = new List<List<KeyValuePair<string, object>>>();
        private List<AbstractCacheTask> _cacheTasks = new List<AbstractCacheTask>();
        private static readonly Logger _logger = Common.Log.NLogUtility.GetCurrentClassLogger();

        private static Dictionary<Type, Func<object, AbstractCacheTask, Task>> _singleTaskFuncs = new Dictionary<Type, Func<object, AbstractCacheTask, Task>>();
        private static Dictionary<Type, Func<object, AbstractCacheTask, Task>> _multipleTaskFuncs = new Dictionary<Type, Func<object, AbstractCacheTask, Task>>();

        static TransactionTask()
        {
            Type singleDbCacheType = typeof(ISingleDBCache<>);
            Type multipleDbCacheType = typeof(IMultipleDBCache<>);
            MethodInfo applySingleMethod = typeof(TransactionTask).GetMethod(nameof(ApplyTaskToSingleController),
                BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo applyMultipleMethod = typeof(TransactionTask).GetMethod(nameof(ApplyTaskToMultipleController),
                BindingFlags.NonPublic | BindingFlags.Static);
            foreach (KeyValuePair<Type, DaoDefinition.DefinitionContext> pair in DaoDefinition.Models)
            {
                Type type = pair.Key;
                if (pair.Value.IsMultipleDbModel == false)
                {
                    Type dbCacheType = singleDbCacheType.MakeGenericType(type);
                    MethodInfo method = applySingleMethod.MakeGenericMethod(type);
                    Type delegateType = typeof(Func<,,>).MakeGenericType(dbCacheType, typeof(AbstractCacheTask), typeof(Task));
                    var invokeFunc = method.CreateDelegate(delegateType);
                    _singleTaskFuncs.Add(type, (controller, task) => (Task)invokeFunc.DynamicInvoke(controller, task));
                }
                else
                {
                    Type dbCacheType = multipleDbCacheType.MakeGenericType(type);
                    MethodInfo method = applyMultipleMethod.MakeGenericMethod(type);
                    Type delegateType = typeof(Func<,,>).MakeGenericType(dbCacheType, typeof(AbstractCacheTask), typeof(Task));
                    var invokeFunc = method.CreateDelegate(delegateType);
                    _multipleTaskFuncs.Add(type, (controller, task) => (Task)invokeFunc.DynamicInvoke(controller, task));
                }
            }
        }

        public TransactionTask(AbstractDao dao, DaoCache daoCache)
        {
            _dao = dao;
            _daoCache = daoCache;
        }

        public void Add(string query, List<KeyValuePair<string, object>> param, AbstractCacheTask task = null)
        {
            _queries.Add(query);
            _params.Add(param);
            if (task != null)
                _cacheTasks.Add(task);
        }

        public async Task<bool> Execute()
        {
            try
            {
                if (_queries.Count <= 0)
                {
                    return true;
                }

                bool result = await _dao.ExecuteSqlTrans(_queries, _params);
                if (result == false)
                {
                    // logging?
                    return false;
                }

                foreach (var cacheTask in _cacheTasks)
                {
                    if (cacheTask.GetControllerType() == AbstractCacheTask.ControllerType.SINGLE)
                    {
                        await _singleTaskFuncs[cacheTask.Type](_daoCache.Get(cacheTask.Type), cacheTask);
                    }
                    else
                    {
                        await _multipleTaskFuncs[cacheTask.Type](_daoCache.Get(cacheTask.Type), cacheTask);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex.Message);
                _logger.Fatal(ex.StackTrace);
            }

            return false;
        }

        private static async Task ApplyTaskToSingleController<T>(ISingleDBCache<T> controller, AbstractCacheTask task)
        {
            switch (task.GetApplyCacheType())
            {
                case AbstractCacheTask.ApplyCacheType.SET:
                    await ApplySetTaskToSingleController(controller, task);
                    break;
                case AbstractCacheTask.ApplyCacheType.DEL:
                default:
                    await ApplyDelTaskToSingleController(controller, task);
                    break;
            }
        }

        private static async Task ApplySetTaskToSingleController<T>(ISingleDBCache<T> controller, AbstractCacheTask task)
        {
            switch (task.GetUpdateColumnType())
            {
                case AbstractCacheTask.UpdateColumnType.PARTIAL:
                    await controller.SetCache(task.CacheParams["mainKey"].ToString(), (HashEntry[])task.CacheParams["changeColumns"]);
                    break;
                case AbstractCacheTask.UpdateColumnType.ALL:
                default:
                    await controller.SetCache((T)task.CacheParams["newValue"]);
                    break;
            }
        }

        private static async Task ApplyDelTaskToSingleController<T>(ISingleDBCache<T> controller, AbstractCacheTask task)
        {
            switch (task.GetUpdateColumnType())
            {
                case AbstractCacheTask.UpdateColumnType.PARTIAL:
                case AbstractCacheTask.UpdateColumnType.ALL:
                default:
                    await controller.DelCache(task.CacheParams["mainKey"].ToString());
                    break;
            }
        }

        private static async Task ApplyTaskToMultipleController<T>(IMultipleDBCache<T> controller, AbstractCacheTask task) where T : Common.Model.IModel
        {
            switch (task.GetApplyCacheType())
            {
                case AbstractCacheTask.ApplyCacheType.SET:
                    await ApplySetTaskToMultipleController(controller, task);
                    break;
                case AbstractCacheTask.ApplyCacheType.DEL:
                    await ApplyDelTaskToMultipleController(controller, task);
                    break;
                default:
                    break;
            }
        }

        private static async Task ApplySetTaskToMultipleController<T>(IMultipleDBCache<T> controller, AbstractCacheTask task) where T : Common.Model.IModel
        {
            string mainKey;
            switch (task.GetUpdateTargetType())
            {
                case AbstractCacheTask.UpdateTargetType.ONE:
                    var newValue = (T)task.CacheParams["newValue"];
                    mainKey = newValue.GetMainKey();
                    await controller.SetCache(mainKey, newValue);
                    break;
                case AbstractCacheTask.UpdateTargetType.LIST:
                    var newValues = (List<T>)task.CacheParams["newValues"];
                    if (newValues.Count == 0) return;
                    mainKey = newValues[0].GetMainKey();
                    await controller.SetListCache(mainKey, newValues);
                    break;
                default:
                    break;
            }
        }

        private static async Task ApplyDelTaskToMultipleController<T>(IMultipleDBCache<T> controller, AbstractCacheTask task) where T : Common.Model.IModel
        {
            switch (task.GetUpdateTargetType())
            {
                case AbstractCacheTask.UpdateTargetType.ONE:
                    await controller.DelCache(task.CacheParams["mainKey"].ToString(), (string)task.CacheParams["subKey"]);
                    break;
                case AbstractCacheTask.UpdateTargetType.LIST:
                    await controller.DelListCache(task.CacheParams["mainKey"].ToString(), (List<string>)task.CacheParams["subKeys"]);
                    break;
                case AbstractCacheTask.UpdateTargetType.ALL:
                    await controller.DelAllCache(task.CacheParams["mainKey"].ToString());
                    break;
                default:
                    break;
            }
        }
    }
}
#endif