# GPP-app development commands

# Run the full stack via Aspire
run:
    aspire run

# Set the LLM API key for woo-hoo metadata generation
set-llm-key key:
    dotnet user-secrets set "Parameters:llmApiKey" "{{key}}" --project ODPC.AppHost

# Show current user secrets
secrets:
    dotnet user-secrets list --project ODPC.AppHost

# Build the .NET solution
build:
    dotnet build

# Type-check the Vue frontend
typecheck:
    cd odpc.client && npx vue-tsc --noEmit

# Install frontend dependencies
npm-install:
    cd odpc.client && npm install

# Build the woo-hoo image locally (until ghcr.io package is public)
build-woo-hoo:
    docker build --platform linux/amd64 --target production -t ghcr.io/infonl/woo-hoo:latest ../woo-hoo

# Reset the database (removes postgres volume so fixtures reload)
reset-db:
    docker ps -aq --filter "name=postgres" --filter "label=com.microsoft.developer.usvc-dev.persistent" | xargs docker rm -f 2>/dev/null || true
    docker volume ls -q | grep 'odpc.*postgres' | xargs docker volume rm 2>/dev/null || true

# Clean up all Aspire persistent containers and volumes
clean:
    docker ps -aq --filter "label=com.microsoft.developer.usvc-dev.creatorProcessStartTime" | xargs docker rm -f 2>/dev/null || true
    docker volume ls -q | grep '^odpc.apphost' | xargs docker volume rm 2>/dev/null || true
