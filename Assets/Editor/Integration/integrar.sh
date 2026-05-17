#!/bin/bash
#===============================================
#   UNITY INTEGRATION ORCHESTRATOR v2.0
#===============================================
# Cross-platform script for automated branch
# integration with namespace injection and
# collision resolution
#===============================================

set -o pipefail

# --- CONFIGURACION ---
MAIN_BRANCH="main"
BACKUP_ENABLED=true
UNITY_INSTALL_KEY="HKEY_LOCAL_MACHINE\\SOFTWARE\\Unity Technologies\\Hub"

# --- DETECCION DE UNITY (Windows) ---
find_unity_windows() {
    local unity_exe=""

    if [ -n "$UNITY_PATH" ] && [ -f "$UNITY_PATH" ]; then
        echo "$UNITY_PATH"
        return 0
    fi

    if [ -f "/c/Program Files/Unity Hub/Editor/6000.3.5f2/Editor/Unity.exe" ]; then
        echo "/c/Program Files/Unity Hub/Editor/6000.3.5f2/Editor/Unity.exe"
        return 0
    fi

    if [ -f "/c/Program Files/Unity/Hub/Editor/2022.3.x/Editor/Unity.exe" ]; then
        echo "/c/Program Files/Unity/Hub/Editor/2022.3.x/Editor/Unity.exe"
        return 0
    fi

    if reg query "$UNITY_INSTALL_KEY" 2>/dev/null; then
        local install_location=$(reg query "$UNITY_INSTALL_KEY" /v "InstallLocation" 2>/dev/null | grep "InstallLocation" | cut -d'=' -f2 | xargs)
        if [ -f "$install_location/Editor/Unity.exe" ]; then
            echo "$install_location/Editor/Unity.exe"
            return 0
        fi
    fi

    local program_files="/c/Program Files"
    local program_files_x86="/c/Program Files (x86)"

    for dir in "$program_files" "$program_files_x86"; do
        if [ -d "$dir" ]; then
            local unity_path=$(find "$dir" -name "Unity.exe" -path "*/Editor/Unity.exe" 2>/dev/null | head -n1)
            if [ -n "$unity_path" ]; then
                echo "$unity_path" | sed 's/\\/\//g'
                return 0
            fi
        fi
    done

    return 1
}

# --- UTILIDADES ---
log() {
    local message="$1"
    local timestamp=$(date +"%H:%M:%S")
    echo -e "\033[1;37m[Integration $timestamp]\033[0m $message"
}

log_success() {
    local message="$1"
    local timestamp=$(date +"%H:%M:%S")
    echo -e "\033[1;32m[Integration $timestamp] SUCCESS:\033[0m $message"
}

log_error() {
    local message="$1"
    local timestamp=$(date +"%H:%M:%S")
    echo -e "\033[1;31m[Integration $timestamp] ERROR:\033[0m $message" >&2
}

log_warning() {
    local message="$1"
    local timestamp=$(date +"%H:%M:%S")
    echo -e "\033[1;33m[Integration $timestamp] WARNING:\033[0m $message"
}

log_info() {
    local message="$1"
    local timestamp=$(date +"%H:%M:%S")
    echo -e "\033[1;36m[Integration $timestamp]\033[0m $message"
}

cleanup_on_error() {
    local branch="$1"
    log_error "Error detected, returning to $MAIN_BRANCH..."
    git checkout "$MAIN_BRANCH" 2>/dev/null
}

sanitize_branch_name() {
    echo "$1" | sed 's/[^a-zA-Z0-9_-]/_/g'
}

# --- INICIO ---
clear
echo -e "\033[1;34m===============================================\033[0m"
echo -e "\033[1;34m   UNITY INTEGRATION ORCHESTRATOR v2.0       \033[0m"
echo -e "\033[1;34m===============================================\033[0m"
echo ""

UNITY_EXE=$(find_unity_windows)
if [ -z "$UNITY_EXE" ]; then
    log_error "No se encontro Unity. Instala Unity Hub o define UNITY_PATH"
    exit 1
fi

log_info "Unity detectado: $UNITY_EXE"
echo ""

read -p "Filtro de busqueda de ramas (ej. nombre-apellido): " PATTERN

if [ -z "$PATTERN" ]; then
    log_error "Patron vacio. Saliendo."
    exit 1
fi

BRANCHES=$(git branch -r 2>/dev/null | grep "$PATTERN" | sed 's/origin\///' | sed 's/\*/''/g' | grep -v "$MAIN_BRANCH")

if [ -z "$BRANCHES" ]; then
    log_error "No se encontraron ramas con el patron: $PATTERN"
    exit 1
fi

CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD 2>/dev/null)
log_info "Rama actual: $CURRENT_BRANCH"
log_info "Rama principal: $MAIN_BRANCH"
echo ""

declare -a BRANCH_ARRAY
while IFS= read -r branch; do
    [ -n "$branch" ] && BRANCH_ARRAY+=("$branch")
done <<< "$BRANCHES"

if [ ${#BRANCH_ARRAY[@]} -eq 0 ]; then
    log_error "No hay ramas validas para procesar"
    exit 1
fi

log_info "Ramas encontradas: ${#BRANCH_ARRAY[@]}"
echo ""

for BRANCH in "${BRANCH_ARRAY[@]}"; do
    SANITIZED_BRANCH=$(sanitize_branch_name "$BRANCH")
    echo ""
    echo -e "\033[1;32m===============================================\033[0m"
    log_info ">>> Procesando rama: $BRANCH"
    echo -e "\033[1;32m===============================================\033[0m"

    git checkout -q "$BRANCH" 2>/dev/null
    if [ $? -ne 0 ]; then
        log_error "No se pudo hacer checkout de $BRANCH"
        continue
    fi

    log "Buscando escenas en Assets (ignorando paquetes)..."
    mapfile -t SCENE_ARRAY < <(find Assets -name "*.unity" -not -path "*/Packages/*" -not -path "*/Editor/*" -not -path "*/WebGLTemplates/*" 2>/dev/null)

    if [ ${#SCENE_ARRAY[@]} -eq 0 ]; then
        log_warning "No se encontraron escenas en la rama $BRANCH"
        git checkout "$MAIN_BRANCH" 2>/dev/null
        continue
    fi

    if [ ! -d "Assets/Editor/Integration" ]; then
        log_info "Trayendo Integration Engine desde $MAIN_BRANCH..."
        mkdir -p "Assets/Editor/Integration"
        git checkout "$MAIN_BRANCH" -- "Assets/Editor/Integration/" 2>/dev/null
        if [ ! -f "Assets/Editor/Integration/IntegrationEngine.cs" ]; then
            log_error "No se pudo traer IntegrationEngine desde $MAIN_BRANCH"
            git checkout "$MAIN_BRANCH" 2>/dev/null
            continue
        fi
        git checkout "$BRANCH" 2>/dev/null
    fi

    echo -e "\033[1;33mEscenas encontradas:\033[0m"
    for i in "${!SCENE_ARRAY[@]}"; do
        SIZE=$(du -h "${SCENE_ARRAY[$i]}" 2>/dev/null | cut -f1 || echo "unknown")
        echo "  [$i] ${SCENE_ARRAY[$i]} (Peso: $SIZE)"
    done

    read -p "Selecciona indice de escena a integrar (o 's' para saltar): " OPT

    if [[ "$OPT" =~ ^[Ss]$ ]]; then
        log_info "Saltando rama $BRANCH..."
        git checkout "$MAIN_BRANCH" 2>/dev/null
        continue
    fi

    if ! [[ "$OPT" =~ ^[0-9]+$ ]] || [ "$OPT" -ge "${#SCENE_ARRAY[@]}" ]; then
        log_error "Indice invalido: $OPT"
        git checkout "$MAIN_BRANCH" 2>/dev/null
        continue
    fi

    SELECTED_SCENE="${SCENE_ARRAY[$OPT]}"
    USER_ID=$(echo "$BRANCH" | cut -d'-' -f1,2 | tr '[:lower:]' '[:upper:]')

    if [ -z "$USER_ID" ]; then
        USER_ID=$(sanitize_branch_name "$BRANCH")
    fi

    log_info "Escena seleccionada: $SELECTED_SCENE"
    log_info "Usuario: $USER_ID"

    TEMP_LOG=$(mktemp /tmp/unity_integration_XXXXXX.log)
    log_info "Log temporal: $TEMP_LOG"

    log_info "Ejecutando Unity en modo batch..."

    "$UNITY_EXE" -batchmode \
        -projectPath "$(pwd)" \
        -executeMethod UnityIntegration.IntegrationEngine.ProcessScene \
        -scenePath "$SELECTED_SCENE" \
        -userName "$USER_ID" \
        -branch "$BRANCH" \
        -quit \
        -nographics \
        -logFile "$TEMP_LOG" 2>&1

    UNITY_EXIT_CODE=$?

    if [ -f "$TEMP_LOG" ]; then
        echo ""
        echo -e "\033[1;33m--- Log de Unity ---\033[0m"
        tail -50 "$TEMP_LOG"
        echo -e "\033[1;33m--- Fin del Log ---\033[0m"
        echo ""
    fi

    if grep -q "INTEGRATION_SUCCESS" "$TEMP_LOG" 2>/dev/null; then
        log_success "Unity proceso completado exitosamente"

        log "Haciendo commit de cambios..."
        git add .

        if git diff --cached --quiet; then
            log_warning "No hay cambios para commitear"
        else
            git commit -m "Automated: Aislamiento y Namespaces para $USER_ID [skip ci]" 2>/dev/null
            log_success "Commit creado"
        fi

        echo ""
        log_info "Fusionando $BRANCH en $MAIN_BRANCH..."
        git checkout "$MAIN_BRANCH" 2>/dev/null

        MERGE_RESULT=$(git merge "$BRANCH" --no-edit 2>&1)
        MERGE_EXIT=$?

        if [ $MERGE_EXIT -eq 0 ]; then
            log_success "Rama $BRANCH integrada exitosamente en $MAIN_BRANCH"
        else
            log_error "Conflicto en merge. Resuelve manualmente:"
            echo "$MERGE_RESULT"
            log_info "Rama $BRANCH disponible para resolucion"
        fi
    else
        log_error "Unity batchmode fallo o no completo la integracion"
        log_error "Revisa el log: $TEMP_LOG"

        if [ "$BACKUP_ENABLED" = true ]; then
            log_info "Buscando backup para rollback..."
            if [ -d ".integration_backup" ]; then
                BACKUP_DIR=$(ls -t .integration_backup 2>/dev/null | head -n1)
                if [ -n "$BACKUP_DIR" ]; then
                    log_info "Backup encontrado: .integration_backup/$BACKUP_DIR"
                fi
            fi
        fi

        git checkout "$MAIN_BRANCH" 2>/dev/null
    fi

    rm -f "$TEMP_LOG" 2>/dev/null

done

echo ""
echo -e "\033[1;34m===============================================\033[0m"
echo -e "\033[1;34m   PROCESO FINALIZADO                           \033[0m"
echo -e "\033[1;34m===============================================\033[0m"
log_success "Todas las ramas han sido procesadas"
echo ""

git checkout "$MAIN_BRANCH" 2>/dev/null
exit 0