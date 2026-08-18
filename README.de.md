# Elin's PEAK

**Languages:** [English](README.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md) · [한국어](README.ko.md) · [Deutsch](README.de.md) · [Español](README.es.md) · [Français](README.fr.md)

## Level einfach alles!

PEAK liefert bereits riskante Kletterpartien, überladene Rucksäcke, Gift, Kälte, Hitze, Erschöpfung und so manchen völlig vermeidbaren Sturz. **Elin's PEAK sorgt dafür, dass dein Charakter aus all dem tatsächlich lernt!**

Das Prinzip ist simpel: **Was du häufig benutzt, wird besser.** Mit schwerer Last zu laufen trainiert Stärke. Klettern trainiert die jeweilige Kletterfähigkeit. Gehen, Sprinten, Springen, Stürze überleben und Statuszustände aushalten werden zu dauerhafter Charakterentwicklung.

Inspiriert ist das System stark von **Elin**. Es gibt keinen einzigen Charakterlevel, der alles bestimmt, und keine allgemeinen Punkte, die nach einer Runde verteilt werden. Fähigkeiten steigen, weil du sie tatsächlich benutzt hast.

Aktuell gibt es **18 dauerhafte Fähigkeiten** mit einer Standardobergrenze von **999**. Der Fortschritt gehört deinem Spieler, wird lokal gespeichert und bleibt über Sitzungen hinweg erhalten.

## Fähigkeiten

### Körper
- **Stärke**: Trainiert beim Bewegen mit Weight. Verringert getragenes Weight und schaltet zusätzliche Rucksackplätze frei.
- **Ausdauer**: Trainiert durch Ausdauerverbrauch. Erhöht echte maximale Ausdauer und Regeneration.
- **Athletik**: Gehen und Sprinten trainieren sie. Verbessert Bodenbewegung, Sprint und Sprint-Effizienz.
- **Beweglichkeit**: Trainiert durch Sprünge. Verbessert Sprungimpuls, Sprungeffizienz und leicht die Luftkontrolle.
- **Vitalität**: Trainiert durch echte Sturzverletzungen. Verringert zukünftige Fall-Injury.

### Klettern
- **Wandklettern**, **Seilklettern** und **Rankenklettern** verbessern jeweils Geschwindigkeit und Ausdauereffizienz.
- **Nassgriff** trainiert auf rutschigen Wänden und reduziert Abwärtszug sowie zugehörigen Ausdauerverbrauch.
- **Kletterzähigkeit** trainiert beim Wandklettern unter 20% normaler Ausdauer und reduziert die dortigen Kontroll-, Rutsch- und Ausdauerstrafen.

### Widerstand
Das blaue **Widerstand**-Feld enthält acht getrennte Fähigkeiten:
**Gift, Kälte, Hitze, Müdigkeit, Sporen, Hunger, Fluch und Versteinerung.**

EP gibt es nur, wenn der passende Zustand tatsächlich zunimmt. Gift, Kälte, Hitze, Müdigkeit und Sporen verbessern zusätzlich natürliche Erholung, sofern PEAK den jeweiligen Abbau als natürliche Erholung markiert.

## Stärke erweitert Rucksäcke

| Stärke | Zusätzliche Rucksackplätze |
|---:|---:|
| 20 | +1 |
| 40 | +2 |
| 70 | +3 |
| 120 | +4 |
| 200 | +5 |

Das Hauptinventar bleibt in Vanilla-Größe. Backpack, Fanny Pack und Jet Pack erhalten dieselben Meilensteine; der Treibstoffslot des Jet Packs bleibt unverändert.

> **BackpackCapacity** sollte deaktiviert bleiben, da beide Mods dieselben Rucksackdaten verändern.

## Dein Fortschritt gehört dir
Fähigkeiten werden lokal gespeichert. Ein Host überschreibt deine Level nicht. Im Airport/Lobby gibt es keine EP. EP in Custom Runs sind standardmäßig deaktiviert.

## Sprache
Die Mod-Oberfläche folgt automatisch der in PEAK ausgewählten Sprache und aktualisiert sich sofort, wenn du sie änderst. Nicht übersetzte Sprachen oder fehlende Übersetzungsschlüssel fallen automatisch auf Englisch zurück.

---

# Technische Details

## XP-Kurve
```text
XP(next) = round(100 * level^1.21)
```
Standard-Maximum: 999.

## Standard-XP
| Fähigkeit | Quelle |
|---|---|
| Ausdauer | 10 EP pro normalisiertem Punkt angeforderter Roh-Ausdauer |
| Stärke | 2 EP pro Roh-Weight × Bewegungsmeter |
| Wand/Seil/Ranke | 8 EP pro gültigem Klettermeter |
| Gehen | 0,22 EP pro Meter |
| Sprint | 1,05 EP pro Meter |
| Beweglichkeit | 8 EP pro gültigem Sprung |
| Vitalität | 100 EP pro normalisiertem Punkt roher Fall-Injury |
| Widerstände | 100 EP pro tatsächlich aufgebautem passenden Statuspunkt |
| Nassgriff | 20 EP pro rutschigem Klettermeter, gewichtet nach Rutschigkeit |
| Kletterzähigkeit | 40 EP pro Wandklettermeter unter 20% normaler Ausdauer |

Positive Boni skalieren meist linear: maximale Ausdauer +0,5%/Stufe, Regeneration +0,1%, Klettertempo +0,3%, Bodenbewegung +0,1%, zusätzlicher Sprint +0,2%, Sprungimpuls +0,15%, Luftkontrolle +0,025%.

Reduktionswerte verwenden eine **verankerte reziproke Kurve** und nähern sich bei Stufe 999 **99,9% Reduktion**.

## Save
```text
%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json
```
Atomare Speicherung plus fünf rotierende Backups.

Source/Issues:
https://github.com/kunrian/Elins-PEAK
