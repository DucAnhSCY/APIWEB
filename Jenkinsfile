pipeline {
    agent any
    
    stages {
        stage('clone'){
            steps {
                echo 'Cloning source code'
                git branch:'master', url: 'https://github.com/DucAnhSCY/APIWEB.git'
            }
        }

        stage('restore package') {
            steps {
                echo 'Restore package'
                bat 'dotnet restore'
            }
        }
        
        stage ('build') {
            steps {
                echo 'build project netcore'
                bat 'dotnet build --configuration Release'
            }
        }
        
        stage ('tests') {
            steps{
                echo 'running test...'
                bat 'dotnet test --no-build --verbosity normal'
            }
        }
        
        stage ('publish to temp folder') {
            steps{
                echo 'Publishing...'
                bat 'dotnet publish -c Release -o ./publish'
            }
        }
        
        stage ('Copy to IIS folder') {
            steps {
                echo 'Copy to running folder'
                powershell '''
                    # Stop IIS site
                    Import-Module WebAdministration
                    if (Get-Website -Name "MySite" -ErrorAction SilentlyContinue) {
                        Stop-Website -Name "MySite"
                        Start-Sleep -Seconds 3
                    }
                    
                    # Copy files
                    Copy-Item -Path "$env:WORKSPACE\\publish\\*" -Destination "c:\\wwwroot\\myproject\\" -Recurse -Force
                    
                    # Start IIS site
                    if (Get-Website -Name "MySite" -ErrorAction SilentlyContinue) {
                        Start-Website -Name "MySite"
                    }
                '''
            }
        }
        
        stage('Deploy to IIS') {
            steps {
                powershell '''
                    # Create website if it doesn't exist
                    Import-Module WebAdministration
                    if (-not (Get-Website -Name "MySite" -ErrorAction SilentlyContinue)) {
                        New-Website -Name "MySite" -Port 81 -PhysicalPath "c:\\wwwroot\\myproject"
                    }
                '''
            }
        }
    }
}