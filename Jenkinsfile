// =============================================================================
// ToDoList API - CI/CD Pipeline
// Builds .NET, runs tests, builds and pushes Docker image, deploys via SSH.
// Staging/Testing from branch 'staging'; Production from 'main' (manual approval).
//
// Required Jenkins configuration:
// - Set DOCKER_REGISTRY (job or global env) to your registry URL, e.g. registry.example.com
// - Create Username/Password credential with id 'docker-registry-credentials' for push
// - SSH credentials as defined in SSH_CRED_126 and SSH_CRED_144
// =============================================================================

pipeline {
  agent {
    docker {
      // Runs the pipeline in a known-good environment that has .NET 8 available.
      // We mount the host Docker socket so `docker build/push` works from inside the agent container.
      image 'mcr.microsoft.com/dotnet/sdk:8.0-jammy'
      args '-v /var/run/docker.sock:/var/run/docker.sock'
      reuseNode true
    }
  }

  options {
    skipDefaultCheckout(true)
    timestamps()
    disableConcurrentBuilds()
    timeout(time: 30, unit: 'MINUTES')
  }

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = "1"

    APP_IMAGE_NAME       = "todolist-api"
    DOCKER_REGISTRY      = "${env.DOCKER_REGISTRY ?: 'registry.example.com'}"  // Override in Jenkins env
    IMAGE_TAG            = "${env.GIT_COMMIT ?: 'unknown'}"

    STAGING_HOST         = "10.50.30.126"
    TESTING_HOST         = "10.50.30.126"
    PRODUCTION_HOST      = "10.50.30.144"

    REMOTE_APP_PATH      = "/opt/todolist"
    REMOTE_TAG_FILE      = ".deployed_image_tag"
    REMOTE_PREVIOUS_TAG  = ".previous_image_tag"

    STAGING_API_PORT     = "8081"
    TESTING_API_PORT     = "8082"
    PRODUCTION_API_PORT  = "8081"

    SSH_CRED_126         = "ssh-posb-10-50-30-126"
    SSH_CRED_144         = "ssh-posb-10-50-30-144"

    SSH_OPTS             = "-o StrictHostKeyChecking=no -o ServerAliveInterval=30 -o ServerAliveCountMax=6 -o ConnectTimeout=15 -o BatchMode=yes"
  }

  stages {
    stage('Prepare Tools') {
      when { anyOf { branch 'staging'; branch 'main' } }
      steps {
        echo "[Prepare Tools] Installing required CLI tools (docker, curl, ssh, git)..."
        sh '''
          set -e
          apt-get update
          DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
            ca-certificates \
            curl \
            git \
            openssh-client \
            docker.io
          rm -rf /var/lib/apt/lists/*
          docker --version
          dotnet --version
          ssh -V || true
          curl --version
        '''
      }
    }

    stage('Checkout') {
      steps {
        echo "[Checkout] Checking out source..."
        checkout scm
        echo "[Checkout] Commit: ${env.GIT_COMMIT}"
      }
    }

    stage('Build') {
      when { anyOf { branch 'staging'; branch 'main' } }
      steps {
        echo "[Build] Restoring and building .NET project (Release)..."
        sh 'dotnet restore'
        sh 'dotnet build --configuration Release --no-restore'
        echo "[Build] Build completed successfully."
      }
    }

    stage('Test') {
      when { anyOf { branch 'staging'; branch 'main' } }
      steps {
        echo "[Test] Running unit tests..."
        sh 'dotnet test --configuration Release --no-build --verbosity normal'
        echo "[Test] All tests passed."
      }
    }

    stage('Docker Build') {
      when { anyOf { branch 'staging'; branch 'main' } }
      steps {
        echo "[Docker Build] Building image ${DOCKER_REGISTRY}/${APP_IMAGE_NAME}:${IMAGE_TAG}"
        sh "docker build -t ${DOCKER_REGISTRY}/${APP_IMAGE_NAME}:${IMAGE_TAG} -t ${APP_IMAGE_NAME}:${IMAGE_TAG} ."
        echo "[Docker Build] Image built successfully."
      }
    }

    stage('Push Image') {
      when { anyOf { branch 'staging'; branch 'main' } }
      steps {
        echo "[Push Image] Pushing to ${DOCKER_REGISTRY}/${APP_IMAGE_NAME}:${IMAGE_TAG}"
        withCredentials([usernamePassword(
          credentialsId: 'docker-registry-credentials',
          usernameVariable: 'REGISTRY_USER',
          passwordVariable: 'REGISTRY_PASS'
        )]) {
          sh """
            echo \"\$REGISTRY_PASS\" | docker login -u \"\$REGISTRY_USER\" --password-stdin ${DOCKER_REGISTRY} || true
            docker push ${DOCKER_REGISTRY}/${APP_IMAGE_NAME}:${IMAGE_TAG}
          """
        }
        echo "[Push Image] Image pushed successfully."
      }
    }

    stage('Deploy to Staging') {
      when { branch 'staging' }
      steps {
        echo "[Deploy] Deploying to Staging (${STAGING_HOST})..."
        script {
          deployToEnvironment(
            host: env.STAGING_HOST,
            credId: env.SSH_CRED_126,
            envName: 'Staging',
            composeProject: 'todolist-staging',
            healthPort: env.STAGING_API_PORT,
            branchName: 'staging'
          )
        }
        echo "[Deploy] Staging deployment completed."
      }
    }

    stage('Deploy to Testing') {
      when { branch 'staging' }
      steps {
        echo "[Deploy] Deploying to Testing (${TESTING_HOST})..."
        script {
          deployToEnvironment(
            host: env.TESTING_HOST,
            credId: env.SSH_CRED_126,
            envName: 'Testing',
            composeProject: 'todolist-testing',
            healthPort: env.TESTING_API_PORT,
            branchName: 'staging'
          )
        }
        echo "[Deploy] Testing deployment completed."
      }
    }

    stage('Deploy to Production (manual)') {
      when { branch 'main' }
      steps {
        script {
          def proceed = input(
            message: "Deploy commit ${env.GIT_COMMIT} to Production (${env.PRODUCTION_HOST})?",
            ok: 'Deploy',
            parameters: []
          )
        }
        echo "[Deploy] Deploying to Production (${PRODUCTION_HOST})..."
        script {
          deployToEnvironment(
            host: env.PRODUCTION_HOST,
            credId: env.SSH_CRED_144,
            envName: 'Production',
            composeProject: 'todolist-prod',
            healthPort: env.PRODUCTION_API_PORT,
            branchName: 'main'
          )
        }
        echo "[Deploy] Production deployment completed."
      }
    }
  }

  post {
    failure {
      echo "[Pipeline] Pipeline failed. Check logs above."
    }
    success {
      echo "[Pipeline] Pipeline finished successfully."
    }
  }
}

// -----------------------------------------------------------------------------
// Deploy to a single environment via SSH: pull image, up, health check, rollback on failure
// -----------------------------------------------------------------------------
void deployToEnvironment(Map args) {
  String host        = args.host
  String credId      = args.credId
  String envName     = args.envName
  String composeProj = args.composeProject
  String healthPort  = args.healthPort
  String branchName  = args.branchName ?: 'staging'

  String fullImage = "${env.DOCKER_REGISTRY}/${env.APP_IMAGE_NAME}:${env.IMAGE_TAG}"
  String healthUrl  = "http://${host}:${healthPort}/health"

  sshagent(credentials: [credId]) {
    sh """
      set -e
      export DOCKER_REGISTRY="${env.DOCKER_REGISTRY}"
      export IMAGE_TAG="${env.IMAGE_TAG}"
      export ASPNETCORE_ENVIRONMENT="${envName}"

      ssh ${env.SSH_OPTS} ${host} /bin/bash -s -- "${env.REMOTE_APP_PATH}" "${env.REMOTE_TAG_FILE}" "${env.REMOTE_PREVIOUS_TAG}" "${fullImage}" "${composeProj}" "${envName}" "${branchName}" "${env.GIT_COMMIT}" << 'REMOTE_SCRIPT'
        set -e
        REMOTE_APP_PATH="\$1"
        TAG_FILE="\$2"
        PREV_FILE="\$3"
        FULL_IMAGE="\$4"
        COMPOSE_PROJECT="\$5"
        ENV_NAME="\$6"
        GIT_BRANCH="\$7"
        GIT_COMMIT="\$8"

        cd "\$REMOTE_APP_PATH" || { echo "Failed to cd to \$REMOTE_APP_PATH"; exit 1; }
        ( git fetch --all && git checkout "\$GIT_BRANCH" && git reset --hard "\$GIT_COMMIT" ) || true

        # Save current image as previous for rollback (full image name)
        if [ -f "\$TAG_FILE" ]; then
          cp "\$TAG_FILE" "\$PREV_FILE"
        fi
        echo "\$FULL_IMAGE" > "\$TAG_FILE"

        export DOCKER_REGISTRY="\${FULL_IMAGE%%/*}"
        export IMAGE_TAG="\${FULL_IMAGE##*:}"
        export ASPNETCORE_ENVIRONMENT="\$ENV_NAME"

        echo "Pulling image \$FULL_IMAGE..."
        docker compose -p "\$COMPOSE_PROJECT" pull api || { echo "Pull failed"; exit 1; }
        echo "Starting services..."
        docker compose -p "\$COMPOSE_PROJECT" up -d api || { echo "Compose up failed"; exit 1; }
        echo "Waiting for application to be ready..."
        sleep 10
REMOTE_SCRIPT
    """

    echo "[Health Check] Verifying ${healthUrl}..."
    def healthOk = false
    def attempts = 0
    def maxAttempts = 12
    while (!healthOk && attempts < maxAttempts) {
      attempts++
      try {
        def resp = sh(
          script: "curl -sf -o /dev/null -w '%{http_code}' --connect-timeout 5 --max-time 10 '${healthUrl}'",
          returnStdout: true
        ).trim()
        if (resp == '200') {
          healthOk = true
          echo "[Health Check] Application healthy (HTTP 200)."
        } else {
          echo "[Health Check] Attempt ${attempts}/${maxAttempts}: got HTTP ${resp}, retrying in 10s..."
          sleep(time: 10, unit: 'SECONDS')
        }
      } catch (Exception e) {
        echo "[Health Check] Attempt ${attempts}/${maxAttempts}: request failed, retrying in 10s..."
        sleep(time: 10, unit: 'SECONDS')
      }
    }

    if (!healthOk) {
      echo "[Health Check] Failed after ${maxAttempts} attempts. Rolling back..."
      sshagent(credentials: [credId]) {
        sh """
          set -e
          ssh ${env.SSH_OPTS} ${host} /bin/bash -s -- "${env.REMOTE_APP_PATH}" "${env.REMOTE_TAG_FILE}" "${env.REMOTE_PREVIOUS_TAG}" "${composeProj}" << 'ROLLBACK_SCRIPT'
            set -e
            REMOTE_APP_PATH="\$1"
            TAG_FILE="\$2"
            PREV_FILE="\$3"
            COMPOSE_PROJECT="\$4"
            cd "\$REMOTE_APP_PATH" || exit 1
            if [ ! -f "\$PREV_FILE" ]; then
              echo "No previous image to roll back to."
              exit 1
            fi
            PREV_FULL_IMAGE=\$(cat "\$PREV_FILE")
            export DOCKER_REGISTRY="\${PREV_FULL_IMAGE%/*}"
            export IMAGE_TAG="\${PREV_FULL_IMAGE##*:}"
            echo "Rolling back to \$PREV_FULL_IMAGE..."
            docker compose -p "\$COMPOSE_PROJECT" pull api && docker compose -p "\$COMPOSE_PROJECT" up -d api
            echo "\$PREV_FULL_IMAGE" > "\$TAG_FILE"
            echo "Rollback completed."
ROLLBACK_SCRIPT
        """
      }
      error("Deployment to ${envName} failed: health check did not pass. Rollback completed.")
    }
  }
}
