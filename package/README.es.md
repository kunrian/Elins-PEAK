# Elin's PEAK

**Languages:** [English](README.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md)

## ¡Sube de nivel absolutamente todo!

PEAK ya te da escaladas al borde de la muerte, mochilas demasiado llenas, veneno, frío, calor, agotamiento y alguna que otra caída perfectamente evitable. **¡Elin's PEAK hace que tu personaje aprenda de todo eso de verdad!**

La idea es sencilla: **si sigues haciendo algo, mejoras en ello.** Moverte con peso entrena Fuerza; escalar mejora el tipo de escalada que estás usando; caminar, esprintar, saltar, sobrevivir a caídas y aguantar aflicciones se convierte en progreso permanente.

El sistema está muy inspirado en **Elin**. No existe un único nivel de personaje que decida todo ni una reserva de puntos genéricos al acabar la partida. Las habilidades suben porque realmente las usaste.

Actualmente hay **18 habilidades persistentes** con un nivel máximo predeterminado de **999**. El progreso pertenece a tu jugador, se guarda localmente y continúa entre sesiones.

## Habilidades

### Físicas
- **Fuerza**: se entrena moviéndote con Weight. Reduce el Weight efectivo y desbloquea espacios de mochila.
- **Resistencia física**: se entrena usando resistencia. Aumenta la resistencia máxima real y su regeneración.
- **Atletismo**: caminar y esprintar la entrenan. Mejora movimiento, sprint y eficiencia del sprint.
- **Agilidad**: se entrena saltando. Mejora impulso, eficiencia del salto y un poco el control aéreo.
- **Vitalidad**: se entrena con lesiones de caída válidas. Reduce futuras Injury por caída.

### Escalada
- **Escalada en pared**, **con cuerda** y **en enredaderas** mejoran velocidad y eficiencia de su tipo.
- **Agarre mojado** se entrena en paredes resbaladizas y reduce el tirón hacia abajo y el gasto relacionado.
- **Tenacidad al escalar** se entrena al seguir escalando por debajo del 20% de resistencia normal y reduce esas penalizaciones.

### Resistencia a aflicciones
El panel azul de **Resistencia** contiene ocho habilidades:
**Veneno, Frío, Calor, Somnolencia, Esporas, Hambre, Maldición y Petrificación.**

Solo ganan EXP cuando la aflicción correspondiente aumenta de verdad. Veneno, Frío, Calor, Somnolencia y Esporas también aceleran las rutas que PEAK marca como recuperación natural.

## Fuerza también amplía las mochilas

| Nivel de Fuerza | Espacios extra |
|---:|---:|
| 20 | +1 |
| 40 | +2 |
| 70 | +3 |
| 120 | +4 |
| 200 | +5 |

El inventario principal conserva el tamaño vanilla. Backpack, Fanny Pack y Jet Pack reciben los mismos hitos; el espacio de combustible del Jet Pack no cambia.

> Mantén **BackpackCapacity** desactivado porque ambos mods modifican los mismos datos de mochila.

## Tu progreso es tuyo
El progreso se guarda localmente. Entrar en el lobby de otra persona no permite que el host sustituya tus niveles. No se gana EXP en Airport/lobby. La EXP en partidas personalizadas está desactivada por defecto.

## Idioma
La interfaz del mod sigue automáticamente el idioma seleccionado en PEAK y se actualiza inmediatamente al cambiarlo. Los idiomas o claves sin traducción utilizan el inglés como alternativa.

---

# Detalles técnicos

## Curva de EXP
```text
XP(next) = round(100 * level^1.21)
```
Máximo predeterminado: 999.

## Fuentes de EXP
| Habilidad | Fuente |
|---|---|
| Resistencia física | 10 EXP por punto normalizado de resistencia bruta solicitada |
| Fuerza | 2 EXP por Weight bruto × metro recorrido |
| Pared/cuerda/enredadera | 8 EXP por metro válido |
| Caminar | 0,22 EXP por metro |
| Sprint | 1,05 EXP por metro |
| Agilidad | 8 EXP por salto válido |
| Vitalidad | 100 EXP por punto normalizado de Injury de caída |
| Resistencias | 100 EXP por punto real de la aflicción correspondiente |
| Agarre mojado | 20 EXP por metro resbaladizo, ponderado por deslizamiento |
| Tenacidad | 40 EXP por metro de pared por debajo del 20% de resistencia normal |

Los bonos positivos suelen ser lineales: resistencia máxima +0,5%/nivel, regeneración +0,1%, velocidad de escalada +0,3%, movimiento terrestre +0,1%, sprint adicional +0,2%, salto +0,15%, control aéreo +0,025%.

Las reducciones usan una **curva recíproca anclada** y se aproximan al **99,9% de reducción** en el nivel 999.

## Guardado
```text
%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json
```
Escrituras atómicas y cinco copias de seguridad rotativas.

Código e incidencias:
https://github.com/kunrian/Elins-PEAK
