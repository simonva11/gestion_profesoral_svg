# Lista de chequeo — Versión 2 (Compuerta 3)

> **No se escribe código con esta lista en rojo.**
> Las casillas las marca **una persona**, con nombre y fecha. Una IA puede
> ayudar a evaluar; **no puede auto-aprobarse** (metodología §3.1).
>
> Se recorre **dos veces**: antes de programar (¿la spec es construible?) y
> antes del tag (¿lo construido corresponde?).

## A. ¿La especificación está completa? *(antes de programar)*

| | Pregunta | ✔ | Revisó |
|---|---|---|---|
| A1 | ¿No queda ningún `[NECESITA ACLARACIÓN]` en el `2_spec.md`? | ☐ | |
| A2 | ¿Cada requisito funcional es **medible**? («funciona bien» no es medible) | ☐ | |
| A3 | ¿Cada requisito es **único**? (ninguno dice lo mismo que otro) | ☐ | |
| A4 | ¿Cada criterio de aceptación dice **cómo se comprueba**, no solo qué se espera? | ☐ | |
| A5 | ¿Están escritas las decisiones que VERSION_2.md deja abiertas (D1–D6)? | ☐ | |
| A6 | ¿La compuerta 2 (chequeo de constitución del `3_plan.md`) está pasada, **artículo por artículo**? | ☐ | |
| A7 | ¿La enmienda del artículo 2 (SQL en la base) quedó escrita en la constitución, y no solo en el plan? | ☐ | |
| A8 | ¿Cada historia H5–H10 tiene **su diagrama de secuencia** debajo? | ☐ | |
| A9 | ¿Las capas de los diagramas son las del código, **con los mismos nombres**? | ☐ | |
| A10 | ¿Están los diagramas obligatorios de la §4: contexto, despliegue, clases, secuencia feliz, ER, **dos de error**, ciclo de validación y fases? | ☐ | |
| A11 | ¿Todos están **embebidos en Mermaid**? (un PNG pegado no cumple) | ☐ | |
| A12 | ¿Cada diagrama tiene su **guía de lectura** de 1 a 3 frases? | ☐ | |

## B. ¿Lo construido corresponde? *(antes del tag)*

| | Pregunta | ✔ | Revisó |
|---|---|---|---|
| B1 | ¿Los 12 criterios del `2_spec.md` §8 están en verde? | ☐ | |
| B2 | ¿`grep` de SQL en `Repositorios/` devuelve **vacío**? | ☐ | |
| B3 | ¿El disparador se comprobó **por la interfaz**, no por consola? | ☐ | |
| B4 | ¿Se comprobó **retirando** un hijo, no solo agregando? | ☐ | |
| B5 | ¿Ninguna respuesta de integridad es 500? | ☐ | |
| B6 | ¿Los desplegables están **llenos** y solo con activos? | ☐ | |
| B7 | ¿La colección de Postman trae la carpeta **«Deben fallar»**? | ☐ | |
| B8 | ¿La regresión de la v1 pasó **completa**? | ☐ | |
| B9 | ¿El código hace algo que la spec **no** dice? (si sí, uno de los dos está mal) | ☐ | |
| B10 | ¿La spec dice algo que el código **no** hace? | ☐ | |
| B11 | ¿Cada diagrama sigue describiendo el código después de programar? | ☐ | |

## C. Git, secretos y entrega

| | Pregunta | ✔ | Revisó |
|---|---|---|---|
| C1 | ¿Los dos repositorios son **privados** y `ccastro2050` es colaborador? | ☐ | |
| C2 | ¿**Ningún** commit directo a `main`? | ☐ | |
| C3 | ¿El trabajo va en **su rama** (`rama-<nombre>`), con commits frecuentes y descriptivos? | ☐ | |
| C4 | ¿Todo entró a `main` por **Pull Request**, aunque el autor y quien integra sean la misma persona? | ☐ | |
| C5 | ¿Hay **cero secretos** en el código? ¿Existe `.env.example` y el `.env` está en `.gitignore`? | ☐ | |
| C6 | ¿Está la `GUIA_IA2.md` con **los prompts que de verdad se usaron**? | ☐ | |
| C7 | ¿Los prompts incluyen **lo que hubo que corregirle a la IA**? | ☐ | |
| C8 | ¿El tag `v2` está sobre el commit que pasa los doce criterios? | ☐ | |

## D. Interpretabilidad *(4 de los 20 puntos — individual)*

> Es la única parte de la nota que no se puede delegar — y trabajando solo,
> tampoco hay con quién repartirla: son todos los archivos.

| | Sobre **cada** archivo del repositorio | ✔ |
|---|---|---|
| D1 | ¿Puedo contar **qué hace** cada archivo mío sin leerlo línea por línea? | ☐ |
| D2 | ¿Puedo decir **por qué así y no de la otra manera**, con lo que se gana y lo que se pierde? | ☐ |
| D3 | ¿Puedo decir **qué pasa si falla**: qué recibe quien llamó y qué ve la persona en la pantalla? | ☐ |
| D4 | ¿Puedo decir **dónde se toca** si mañana hay que agregar X — y dónde **no**? | ☐ |
| D5 | ¿Mis comentarios dicen **por qué**, no **qué**? (tape el código: ¿el comentario sigue diciendo algo?) | ☐ |
| D6 | ¿**Comprobé** que los comentarios que puso la IA son ciertos? (comenta lo que *cree* que hizo) | ☐ |
| D7 | ¿Hice fallar mi propio código a propósito y miré el error? | ☐ |

---

**Firmado por:** ______________________  **Fecha:** ____ / ____ / ________

> Firmar esta lista es decir que se recorrió, no que se leyó.
