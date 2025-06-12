
pipeline {
    agent any
    
    stages {
        stage('clone'){
            steps {
                echo 'Cloning source code'
                git branch:'main', url: 'https://github.com/DucAnhSCY/APIWEB.git'
            }
        } // end clone

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
                // iisreset /stop // stop iis de ghi de file 
                bat 'xcopy "%WORKSPACE%\\publish\\*" "c:\\wwwroot\\myproject\\" /E /Y /I /R'
            }
        }
        
        stage('Deploy to IIS') {
            steps {
                powershell '''
                    # Tạo website nếu chưa có
                    Import-Module WebAdministration
                    if (-not (Get-Website -Name "MySite" -ErrorAction SilentlyContinue)) {
                        New-Website -Name "MySite" -Port 81 -PhysicalPath "c:\\wwwroot\\myproject"
                    }
                '''
            }
        } // end deploy iis
    } // end stages
} // end pipeline
