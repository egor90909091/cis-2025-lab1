pipeline {
    agent any

    environment {
        DOTNET_VERSION = '8.0'
        NODE_VERSION = '20'
        ALLURE_VERSION = '2.24.1'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build Frontend') {
            agent {
                docker {
                    image "node:${NODE_VERSION}-alpine"
                }
            }
            steps {
                dir('frontend') {
                    sh 'npm ci'
                    sh 'npm run build'
                }
            }
        }

        stage('Build Backend') {
            agent {
                docker {
                    image "registry.access.redhat.com/ubi8/dotnet-80:8.0"
                }
            }
            steps {
                dir('backend') {
                    sh 'dotnet restore'
                    sh 'dotnet build --configuration Release --no-restore'
                }
            }
        }

        stage('Test Frontend') {
            agent {
                docker {
                    image "node:${NODE_VERSION}-alpine"
                }
            }
            steps {
                dir('frontend') {
                    script {
                        try {
                            sh 'npm run test -- --run --reporter=allure-vitest/reporter'
                        } catch (Exception e) {
                            echo "Frontend tests failed: ${e.getMessage()}"
                            currentBuild.result = 'UNSTABLE'
                        }
                    }
                }
            }
            post {
                always {
                    dir('frontend') {
                        script {
                            def hasAllureResults = sh(
                                script: 'test -d allure-results && [ "$(find allure-results -type f 2>/dev/null | wc -l)" -gt 0 ]',
                                returnStatus: true
                            ) == 0

                            if (hasAllureResults) {
                                echo "Stashing frontend/allure-results"
                                stash name: 'frontend-allure-results', includes: 'allure-results/**', allowEmpty: false
                            } else {
                                echo "No files in frontend/allure-results, skipping stash"
                            }
                        }
                    }
                }
            }
        }

        stage('Test Backend') {
            agent {
                docker {
                    image "registry.access.redhat.com/ubi8/dotnet-80:8.0"
                }
            }
            environment {
                ALLURE_RESULTS_DIR = "${WORKSPACE}/backend/allure-results"
            }
            steps {
                dir('backend') {
                    script {
                        try {
                            sh '''
                                mkdir -p allure-results
                                export ALLURE_RESULTS_DIR=${WORKSPACE}/backend/allure-results
                                echo "ALLURE_RESULTS_DIR is set to: ${ALLURE_RESULTS_DIR}"
                                echo "Current directory: $(pwd)"
                                dotnet test Tests/Tests.csproj --configuration Release --verbosity normal
                                echo "Contents of allure-results:"
                                ls -la allure-results/ || echo "allure-results directory is empty or does not exist"
                            '''
                        } catch (Exception e) {
                            echo "Backend tests failed: ${e.getMessage()}"
                            currentBuild.result = 'UNSTABLE'
                        }
                    }
                }
            }
            post {
                always {
                    dir('backend') {
                        script {
                            def hasAllureResults = sh(
                                script: 'test -d allure-results && [ "$(find allure-results -type f 2>/dev/null | wc -l)" -gt 0 ]',
                                returnStatus: true
                            ) == 0

                            if (hasAllureResults) {
                                echo "Stashing backend/allure-results"
                                stash name: 'backend-allure-results', includes: 'allure-results/**', allowEmpty: false
                            } else {
                                echo "No files in backend/allure-results, skipping stash"
                            }
                        }
                    }
                }
            }
        }

        stage('Prepare Allure Results') {
            steps {
                script {
                    sh 'mkdir -p allure-results'

                    echo "=== Restoring Allure results from previous stages ==="

                    // Восстанавливаем результаты frontend тестов
                    script {
                        try {
                            unstash 'frontend-allure-results'
                            sh '''
                                if [ -d "frontend/allure-results" ]; then
                                    echo "Restored frontend/allure-results, copying..."
                                    cp -v frontend/allure-results/*.json allure-results/ 2>&1 | head -20 || true
                                    echo "Frontend results copied"
                                fi
                            '''
                        } catch (Exception e) {
                            echo "No frontend allure results to restore: ${e.getMessage()}"
                        }
                    }

                    // Восстанавливаем результаты backend тестов
                    script {
                        try {
                            unstash 'backend-allure-results'
                            sh '''
                                if [ -d "backend/allure-results" ]; then
                                    echo "Restored backend test results, copying..."
                                    find backend/allure-results -type f \\( -name "*.json" -o -name "*.txt" -o -name "*.xml" \\) -exec cp -v {} allure-results/ \\; 2>&1 | head -20 || true
                                    echo "Backend results copied"
                                fi
                            '''
                        } catch (Exception e) {
                            echo "No backend test results to restore: ${e.getMessage()}"
                        }
                    }

                    // Создаем environment.properties для Allure (в конце, чтобы не перезаписать результаты)
                    sh '''
                        cat >> allure-results/environment.properties <<EOF
                        Branch=${BRANCH_NAME}
                        Build.Number=${BUILD_NUMBER}
                        Build.URL=${BUILD_URL}
                        Commit=${GIT_COMMIT}
                        Node.Version=20
                        DotNet.Version=8.0
                        EOF
                    '''
                }
            }
        }

        stage('Publish Allure Report') {
            steps {
                script {
                    def hasResults = sh(
                        script: 'test -d allure-results && [ "$(ls -A allure-results 2>/dev/null)" ]',
                        returnStatus: true
                    ) == 0

                    if (hasResults) {
                        echo "Publishing Allure report..."
                        allure([
                            includeProperties: false,
                            jdk: '',
                            properties: [],
                            reportBuildPolicy: 'ALWAYS',
                            results: [[path: 'allure-results']]
                        ])
                    } else {
                        echo "No Allure results found, skipping report generation"
                    }
                }
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'allure-results/**,backend/allure-results/**,frontend/allure-results/**', fingerprint: true, allowEmptyArchive: true
        }
        failure {
            echo 'Pipeline failed. Check the test results in Allure Report.'
        }
        success {
            echo 'Pipeline succeeded! All tests passed.'
        }
    }
}
