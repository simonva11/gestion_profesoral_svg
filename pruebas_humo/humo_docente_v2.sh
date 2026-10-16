#!/usr/bin/env bash
# =====================================================================
#  Prueba de humo de la V2 — recurso `docente`
#
#  Recorre el ciclo completo contra la API levantada, e incluye LAS
#  PETICIONES QUE TIENEN QUE FALLAR: un criterio solo está probado si se
#  probó el desenlace malo (6_contracts.md §5).
#
#  Uso:  bash pruebas_humo/humo_docente_v2.sh
# =====================================================================
set -u
API="${API:-http://localhost:8074}"
CEDULA=1017245
FALLOS=0

# Pide la URL y compara el código de estado con el esperado. Es la línea
# que PUEDE FALLAR: sin la comparación, el script narra pero no prueba.
revisar() {
  local esperado="$1" metodo="$2" ruta="$3" cuerpo="${4:-}" que="$5"
  local codigo
  if [ -n "$cuerpo" ]; then
    codigo=$(curl -s -o /tmp/humo_resp -w "%{http_code}" -X "$metodo" \
      -H "Content-Type: application/json" -d "$cuerpo" "$API$ruta")
  else
    codigo=$(curl -s -o /tmp/humo_resp -w "%{http_code}" -X "$metodo" "$API$ruta")
  fi

  if [ "$codigo" = "$esperado" ]; then
    echo "[OK]    $que  ($metodo $ruta -> $codigo)"
  else
    echo "[ERROR] $que  ($metodo $ruta -> $codigo, se esperaba $esperado)"
    echo "        respuesta: $(head -c 300 /tmp/humo_resp)"
    FALLOS=$((FALLOS + 1))
  fi
}

DOCENTE=$(cat <<JSON
{"cedula":$CEDULA,"nombres":"Ana","apellidos":"Torres Gomez","genero":"Femenino",
 "cargo":"Titular","fechaNacimiento":"1980-04-12","correo":"ana.torres@umv.edu.co",
 "telefono":"3001234567","urlCvlac":"https://cvlac.minciencias.gov.co/ana",
 "fechaActualizacion":"2026-01-30","escalafon":"Asociado","perfil":"Doctora en Ingenieria",
 "convMinciencia":"2023","nacionalidaad":"Colombiana","lineaInvestigacionPrincipal":null}
JSON
)

echo "=== Prueba de humo — docente (v2) contra $API ==="

revisar 200 GET  "/"                              ""          "La API responde"
revisar 200 GET  "/api/linea-investigacion"       ""          "El desplegable tiene de donde sacar opciones (criterio 5)"

echo "--- el camino feliz ---"
revisar 200 POST "/api/docente"        "$DOCENTE"             "Crear un docente SIN linea: «(ninguna)» -> null (criterio 6)"
revisar 200 GET  "/api/docente"                   ""          "Listar: ya hay al menos uno"
revisar 200 GET  "/api/docente/$CEDULA"           ""          "Consultar por cedula"
revisar 200 PATCH "/api/docente/$CEDULA" '{"cargo":"Emerito"}' "Modificar solo un campo"

echo "--- LAS QUE TIENEN QUE FALLAR ---"
revisar 409 POST "/api/docente"        "$DOCENTE"             "Cedula repetida -> 409, NO 500 (criterio 7)"
revisar 422 POST "/api/docente"        '{"cedula":99}'        "Cuerpo incompleto -> 422 con errores[]"
revisar 404 GET  "/api/docente/999999"            ""          "Cedula inexistente -> 404"
revisar 400 GET  "/api/docente?limite=0"          ""          "limite = 0 -> 400 (regla, no forma)"
revisar 400 PATCH "/api/docente/$CEDULA" '{}'                 "PATCH sin campos -> 400, no 404"
revisar 409 POST "/api/docente" \
  "$(echo "$DOCENTE" | sed 's/"cedula":1017245/"cedula":1098332/; s/"lineaInvestigacionPrincipal":null/"lineaInvestigacionPrincipal":987654/')" \
  "Linea de investigacion inexistente -> 409 (clave foranea)"

echo "--- el borrado logico ---"
revisar 200 DELETE "/api/docente/$CEDULA"         ""          "Retirar el docente (UPDATE activo = 0)"
revisar 404 GET    "/api/docente/$CEDULA"         ""          "Un docente retirado responde 404 (criterio 10)"
revisar 404 DELETE "/api/docente/$CEDULA"         ""          "Retirar dos veces -> 404"

echo
if [ "$FALLOS" -eq 0 ]; then
  echo "=== Prueba de humo COMPLETADA CON EXITO ==="
  exit 0
else
  echo "=== Prueba de humo FALLIDA: $FALLOS comprobacion(es) en rojo ==="
  exit 1
fi
