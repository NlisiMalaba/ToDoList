// Jenkins pipeline: build once on main, promote image through
// Staging → Testing → Production using docker-compose deployments.
//
// Requirements:
// - Jenkins agent with: git, Docker, docker compose, .NET SDK.
// - Docker registry credentials in Jenkins (DockerHub/ECR/ACR, etc).
// - SSH access from Jenkins to each environment VM.
// - Per-environment env files already on the VMs:
//   - /secure/todolist/staging.env
//   - /secure/todolist/testing.env
//   - /secure/todolist/production.env

pipeline {
  agent any

  options {
    timestamps()
    disableConcurrentBuilds()
    timeout(time: 30, unit: 'MINUTES')
  }

  triggers {
    // Recommended: use webhooks instead of cron if possible.
    pollSCM('H/5 * * * *')
  }

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = "1"

    // Registry (configure in Jenkins global env or per-job)
    DOCKER_REGISTRY = "${env.DOCKER_REGISTRY ?: 'local'}"
    DOCKER_IMAGE    = "${env.DOCKER_IMAGE ?: 'todolist-api'}"

    // SSH + remote paths (configure per environment in Jenkins)
    STAGING_HOST          = "${env.STAGING_HOST ?: 'staging-host'}"
    TESTING_HOST          = "${env.TESTING_HOST ?: 'testing-host'}"
    PROD_HOST             = "${env.PROD_HOST   ?: 'prod-host'}"
    REMOTE_APP_PATH       = "${env.REMOTE_APP_PATH ?: '/opt/ToDoList'}"
    REMOTE_ENV_STAGING    = "${env.REMOTE_ENV_STAGING ?: '/secure/todolist/staging.env'}"
    REMOTE_ENV_TESTING    = "${env.REMOTE_ENV_TESTING ?: '/secure/todolist/testing.env'}"
    REMOTE_ENV_PRODUCTION = "${env.REMOTE_ENV_PRODUCTION ?: '/secure/todolist/production.env'}"

    // Credential IDs (configure in Jenkins Credentials)
    DOCKER_CREDS_ID = "${env.DOCKER_CREDS_ID ?: 'docker-registry-creds'}"
    SSH_CREDS_ID    = "${env.SSH_CREDS_ID    ?: 'ssh-deploy'}"

    SSH_OPTS = "-o StrictHostKeyChecking=no -o ServerAliveInterval=30 -o ConnectTimeout=15 -o BatchMode=yes"
  }

  stages {
    stage('Checkout') {
      when { branch 'main' }
      steps {
        checkout scm
        script {
          echo "Commit: ${env.GIT_COMMIT}"
        }
      }
    }

    stage('Build & Test') {
      when { branch 'main' }
      steps {
        sh 'dotnet restore src/ToDoList.Api/ToDoList.Api.csproj'
        sh 'dotnet build src/ToDoList.Api/ToDoList.Api.csproj --configuration Release --no-restore'
        sh 'dotnet test src/ToDoList.UnitTests/ToDoList.UnitTests.csproj --configuration Release --verbosity normal --no-build'
      }
    }

    stage('Build & Push Image') {
      when { branch 'main' }
      steps {
        script {
          // Use short SHA as part of immutable tag
          def shortSha = env.GIT_COMMIT.take(7)
          env.IMAGE_TAG = "${shortSha}"
          def fullImage = "${env.DOCKER_REGISTRY}/${env.DOCKER_IMAGE}:${env.IMAGE_TAG}"
          echo "Building image: ${fullImage}"

          docker.withRegistry('', DOCKER_CREDS_ID) {
            sh """
              docker build -t ${fullImage} .
              docker push ${fullImage}
            """
          }
        }
      }
    }

    stage('Deploy to Staging') {
      when { branch 'main' }
      steps {
        script {
          deployToEnv(
            envName: 'staging',
            host: env.STAGING_HOST,
            remotePath: env.REMOTE_APP_PATH,
            envFile: env.REMOTE_ENV_STAGING,
            imageTag: env.IMAGE_TAG
          )
        }
      }
    }

    stage('Approve: Promote to Testing') {
      when { branch 'main' }
      steps {
        timeout(time: 14, unit: 'DAYS') {
          input message: "Promote image tag ${env.IMAGE_TAG} to TESTING?", ok: "Promote"
        }
      }
    }

    stage('Deploy to Testing') {
      when { branch 'main' }
      steps {
        script {
          deployToEnv(
            envName: 'testing',
            host: env.TESTING_HOST,
            remotePath: env.REMOTE_APP_PATH,
            envFile: env.REMOTE_ENV_TESTING,
            imageTag: env.IMAGE_TAG
          )
        }
      }
    }

    stage('Approve: Promote to Production') {
      when { branch 'main' }
      steps {
        timeout(time: 14, unit: 'DAYS') {
          input message: "Promote image tag ${env.IMAGE_TAG} to PRODUCTION?", ok: "Promote"
        }
      }
    }

    stage('Deploy to Production') {
      when { branch 'main' }
      steps {
        script {
          deployToEnv(
            envName: 'production',
            host: env.PROD_HOST,
            remotePath: env.REMOTE_APP_PATH,
            envFile: env.REMOTE_ENV_PRODUCTION,
            imageTag: env.IMAGE_TAG
          )
        }
      }
    }
  }

  post {
    failure {
      echo 'Pipeline failed. Check logs above.'
    }
    success {
      echo "Pipeline completed successfully for image tag ${env.IMAGE_TAG ?: 'N/A'}."
    }
  }
}

// Deploy a specific image tag to a given environment VM using docker compose.
void deployToEnv(Map args) {
  def envName   = args.envName
  def host      = args.host
  def remoteDir = args.remotePath
  def envFile   = args.envFile
  def imageTag  = args.imageTag

  if (!imageTag) {
    error "IMAGE_TAG is not set; cannot deploy."
  }

  echo "Deploying image tag ${imageTag} to ${envName} on ${host}..."

  sshagent(credentials: [env.SSH_CREDS_ID]) {
    sh """
      set -e
      ssh ${env.SSH_OPTS} ${host} /bin/bash -s -- "${remoteDir}" "${envFile}" "${env.DOCKER_REGISTRY}" "${env.DOCKER_IMAGE}" "${imageTag}" << 'REMOTE_SCRIPT'
        set -e
        REMOTE_PATH="\$1"
        ENV_FILE="\$2"
        REGISTRY="\$3"
        IMAGE_NAME="\$4"
        TAG="\$5"

        mkdir -p "\$REMOTE_PATH"
        cd "\$REMOTE_PATH"

        if [ ! -f docker-compose.yml ]; then
          echo "ERROR: docker-compose.yml not found in \$REMOTE_PATH. Please deploy the repo to the server first."
          exit 1
        fi

        export DOCKER_REGISTRY="\$REGISTRY"
        export IMAGE_TAG="\$TAG"

        docker compose -p "todolist-${envName}" --env-file "\$ENV_FILE" -f docker-compose.yml -f docker-compose.${envName}.yml pull
        docker compose -p "todolist-${envName}" --env-file "\$ENV_FILE" -f docker-compose.yml -f docker-compose.${envName}.yml up -d

        docker compose -p "todolist-${envName}" ps
REMOTE_SCRIPT
    """
  }

  echo "Deploy to ${envName} completed."
}

