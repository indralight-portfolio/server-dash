Param(
    [string]$Source = "dev",
    [string]$Database
)
[string]$conn=""
$arrDatabase= @("GameDB")

function test {
    Write-Host $Source $Database
}

function GetConnectionString($Database) {
    if( -Not ( $arrDatabase -CContains $Database ) )
    {
        Write-Host "'$Database' is invalid Database"
        return $FALSE
    }

    switch ( $Source )
    {
        "dev" {
            $db_host="" 		
            return "server=$db_host;port=3306;uid=;pwd=;database=$Database"
        }
        "dev2" {
            $db_host=""
            return "server=$db_host;port=3306;uid=;pwd=;database=$Database"
        }
        default { 
            Write-Host "'$Source' is invalid Source"
            exit
        }
    }
}

function Scaffold ($Database) {
    $conn = GetConnectionString($Database)
    if ( -Not $conn )
    {
        Write-Host "Invalid Connection"
        return
    }
    #Write-Host $conn
    Scaffold-DbContext "$conn" Pomelo.EntityFrameworkCore.MySql -v -f -Project Dash -DataAnnotations -OutputDir ../Lib-Dash/Dash/Model/Rdb -ContextDir ../Lib-Dash/Dash/Server/Dao
}

if(-Not $Database)
{
    foreach ( $Database in $arrDatabase.GetEnumerator() ) {
        #Write-Host $Database
        Scaffold $Database
    }    
}
else
{
    Write-Host $Database
    Scaffold $Database
}
