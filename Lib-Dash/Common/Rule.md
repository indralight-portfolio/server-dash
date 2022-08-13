# Common
 - 프레임 워크에 의존하지 않아야 함. .Net Framework 4.X, .Net Core 2.X 모두에서 동작해야 함.

# Common.Unity
 - 모든 소스의 제일 첫줄과 끝줄에 #if Common_Unity / #endif 추가
 - Unity Engine 각 platform 별 Scripting Define Symbols 에 Common_Unity 추가
 
# Common.NetCore
 - 모든 소스의 제일 첫줄과 끝줄에 #if Common_NetCore / #endif 추가
 - 상위 프로젝트의 Define 에 Common_NetCore 추가