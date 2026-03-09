pipeline {
  agent any

  options {
    skipDefaultCheckout(true)
    timestamps()
  }

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = "1"

    APP_IMAGE       = "todolist-api"
    STAGING_HOST    = "10.50.30.126"
    TESTING_HOST    = "10.50.30.126"
    PRODUCTION_HOST = "10.50.30.144"

    REMOTE_APP_PATH = "/opt/todolist"

    SSH_CRED_126 = "ssh-posb-10-50-30-126"
    SSH_CRED_144 = "ssh-posb-10-50-30-144"
  }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Build') {
      when {
        branch 'staging'
      }
      steps {
        sh 'dotnet restore'
        sh 'dotnet build --configuration Release'
      }
    }

    stage('Unit tests') {
      when {
        branch 'staging'
      }
      steps {
        sh 'dotnet test --configuration Release --no-build'
      }
    }

    stage('Build Docker image') {
      when {
        branch 'staging'
      }
      steps {
        script {
          def imageTag = "${env.GIT_COMMIT}"
          env.IMAGE_TAG = imageTag
          sh "docker build -t ${APP_IMAGE}:${imageTag} ."
        }
      }
    }

    stage('Deploy to Staging') {
      when {
        branch 'staging'
      }
      steps {
        sshagent(credentials: [env.SSH_CRED_126]) {
          sh """
            ssh -o StrictHostKeyChecking=no ${STAGING_HOST} \\
              'cd ${REMOTE_APP_PATH} && \\
               git fetch --all && git checkout staging && git reset --hard ${GIT_COMMIT} && \\
               ASPNETCORE_ENVIRONMENT=Staging docker compose -p todolist-staging up -d --build'
          """
        }
      }
    }

    stage('Deploy to Testing') {
      when {
        branch 'staging'
      }
      steps {
        sshagent(credentials: [env.SSH_CRED_126]) {
          sh """
            ssh -o StrictHostKeyChecking=no ${TESTING_HOST} \\
              'cd ${REMOTE_APP_PATH} && \\
               git fetch --all && git checkout staging && git reset --hard ${GIT_COMMIT} && \\
               ASPNETCORE_ENVIRONMENT=Testing docker compose -p todolist-testing up -d --build'
          """
        }
      }
    }

    stage('Deploy to Production (manual)') {
      when {
        branch 'main'
      }
      steps {
        script {
          def proceed = input message: 'Deploy current main commit to Production (10.50.30.144)?', ok: 'Deploy'
        }

        sshagent(credentials: [env.SSH_CRED_144]) {
          sh """
            ssh -o StrictHostKeyChecking=no ${PRODUCTION_HOST} \\
              'cd ${REMOTE_APP_PATH} && \\
               git fetch --all && git checkout main && git reset --hard ${GIT_COMMIT} && \\
               ASPNETCORE_ENVIRONMENT=Production docker compose -p todolist-prod up -d --build'
          """
        }
      }
    }
  }
}

