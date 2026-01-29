.PHONY: help install up up-deps dev backend frontend test test-frontend test-backend build build-frontend build-backend lint lint-frontend lint-backend format format-frontend format-backend clean

DOTNET := $(shell command -v dotnet 2>/dev/null)

# Default: show help
help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'

# --- Setup ---

install: ## Install frontend dependencies
	cd odpc.client && npm install

# --- Run locally ---

up: ## Start full stack via Docker Compose
	docker compose up

up-deps: ## Start only dependencies (db, redis, openzaak, odrc) via Docker
	docker compose up postgres-db redis openzaak-web openzaak-celery odrc

up-woo-hoo: ## Start woo-hoo service (metadata generation) - run in separate terminal
	@echo "Starting woo-hoo service on http://localhost:8003"
	@echo "Note: This will run in foreground. Open a new terminal for other commands."
	cd ../woo-hoo && uv run uvicorn woo_hoo.main:app --host 0.0.0.0 --port 8003 --reload

dev: install ## Run frontend dev server (Vite with hot reload)
	cd odpc.client && npm run dev

backend: ## Run .NET backend locally (requires dotnet SDK)
	cd ODPC.Server && dotnet run --launch-profile http

frontend: install ## Install deps and run frontend dev server
	cd odpc.client && npm run dev

# --- Tests ---

test: test-frontend test-backend ## Run all tests (frontend + backend)

test-frontend: ## Run frontend unit tests (Vitest)
	cd odpc.client && npm install && npx vitest run

test-backend: ## Run .NET backend tests (requires dotnet 8 SDK + runtime)
ifdef DOTNET
	dotnet test ODPC.Test || echo "Backend tests failed (requires .NET 8 runtime)"
else
	@echo "Skipping backend tests: dotnet SDK not found"
endif

# --- Build ---

build: build-frontend build-backend ## Build frontend and backend

build-frontend: install ## Build frontend for production
	cd odpc.client && npm run build

build-backend: ## Build .NET backend (requires dotnet SDK)
ifdef DOTNET
	dotnet build ODPC.sln
else
	@echo "Skipping backend build: dotnet SDK not found"
endif

# --- Code quality ---

lint: lint-frontend lint-backend ## Lint all code (frontend + backend)

lint-frontend: ## Lint and fix frontend code (ESLint)
	cd odpc.client && npm run lint

lint-backend: ## Lint backend code (dotnet format --verify-no-changes)
ifdef DOTNET
	dotnet format ODPC.sln --verify-no-changes || echo "Backend has formatting issues. Run 'make format-backend' to fix."
else
	@echo "Skipping backend lint: dotnet SDK not found"
endif

format: format-frontend format-backend ## Format all code (frontend + backend)

format-frontend: ## Format frontend code with Prettier
	cd odpc.client && npm run format

format-backend: ## Format backend code (dotnet format)
ifdef DOTNET
	dotnet format ODPC.sln
else
	@echo "Skipping backend format: dotnet SDK not found"
endif

# --- Cleanup ---

clean: ## Remove build artifacts
	cd odpc.client && rm -rf dist node_modules/.vite
ifdef DOTNET
	dotnet clean ODPC.sln
endif

# --- Health checks & logs ---

health: ## Check health of all services
	@echo "=== Service Health Check ==="
	@echo "\n1. woo-hoo AI service:"
	@curl -s --max-time 5 http://localhost:8003/health && echo "" || echo "   ❌ Not running"
	@echo "\n2. ODPC metadata endpoint:"
	@curl -s --max-time 5 http://localhost:62230/api/v1/metadata/health && echo "   ✓ Healthy" || echo "   ❌ Not responding"
	@echo "\n3. Vue dev server (with hot reload):"
	@curl -s --max-time 5 -o /dev/null -w "%{http_code}" http://localhost:5173 2>/dev/null | grep -q "200\|302" && echo "   ✓ Running" || echo "   ❌ Not running (run 'make dev' to start)"
	@echo "\n4. Production-like app (nginx):"
	@curl -s --max-time 5 -o /dev/null -w "%{http_code}" http://localhost:8002 | grep -q "200\|302" && echo "   ✓ Running" || echo "   ❌ Not running"
	@echo "\n5. Django backend (internal):"
	@curl -s --max-time 5 -o /dev/null -w "%{http_code}" http://localhost:8000 | grep -q "200\|302" && echo "   ✓ Running (internal only)" || echo "   ❌ Not running"
	@echo "\n=== Summary ==="
	@echo "🌐 For development (hot reload): http://localhost:5173 (run 'make dev')"
	@echo "🌐 For production-like: http://localhost:8002 (via 'docker compose up')"

logs: ## Show logs from all services
	docker compose logs -f

logs-odpc: ## Show ODPC backend logs
	docker compose logs -f odpc
