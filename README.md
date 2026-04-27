1# 🛠️ AdminTool2 - Mein System-Monitor

Ein kleines, praktisches Tool, um Hardware-Daten auszulesen und Software-Updates zu verwalten. Ich wollte eine einfache, übersichtliche Lösung für den Eigenbedarf basteln.

## 🚀 Hintergrund zum Projekt
Ich bin Hobby-Bastler und hatte früher schon mal ein bisschen was mit **Visual Basic** zu tun – die Logik hinter Programmen war mir also nicht ganz fremd. Allerdings war **C# und WPF** für mich bis vor kurzem komplettes Neuland. 

Mein Ziel bei diesem Projekt war es, mein altes Wissen aus VB-Zeiten zu reaktivieren und direkt auf das moderne .NET-Ökosystem zu übertragen. Anstatt trockene Theorie zu büffeln, wollte ich in wenigen Tagen ein echtes Tool bauen, das unter der Haube deutlich mehr Power hat als meine alten Bastel-Projekte.

Das Ergebnis ist dieses "Einstiegs-Projekt" in die C#-Welt. Es war ein wilder Ritt durch XAML-Templates, Styles und moderne System-APIs.

## ✨ Features
* **Hardware-Check:** Zeigt CPU, Grafikkarte, RAM, Mainboard und Netzwerk in einer Baumstruktur.
* **Live-Werte:** CPU-Last und RAM-Verbrauch werden in Echtzeit angezeigt.
* **Festplatten:** Visualisierte Belegung deiner Laufwerke mit schicken Balken.
* **Modernes Design:** Abgerundete Ecken und ein Dark Mode (war ein ziemlicher Kampf im XAML, sieht aber jetzt gut aus).

## ⚠️ WinGet & Baustellen
Die Update-Funktion nutzt WinGet. Da das Ganze noch ein bisschen experimentell ist, macht WinGet manchmal Probleme – zum Beispiel bei der Versionserkennung oder wenn die Paketquellen mal wieder hängen. Das ist aktuell noch eine "Work-in-Progress"-Ecke.

## 🛠️ Technik-Stack
* **Sprache:** C# 
* **UI:** WPF (.NET 9.0) – komplett eigene Styles, ohne fertige Frameworks.
* **Struktur:** Nutzt das neue `.slnx` Solution-Format.

## 📝 Lizenz
Das Tool steht unter der MIT-Lizenz. Reinschauen, lernen, umbauen – alles erlaubt.

---
**Gebastelt von denneonblue** – Logik war bekannt, C# war neu, das Tool steht.