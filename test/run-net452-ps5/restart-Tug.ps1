function restart-tug {
    try
        {get-process -name "Tug.Server" -ErrorAction Stop
        Stop-Process -name "Tug.Server"
        }
    catch {}
    Set-Location -path "C:\users\Administrator\Documents\Github\tug\Test\run-net452-ps5"
    iex ".\tug.cmd"
    }
    