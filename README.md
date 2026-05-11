# 🛠️ AdminTool2 | System Monitor & Management Dashboard

Ein kompaktes, performantes WPF-Dashboard zur Überwachung von Hardware-Metriken und zur Verwaltung von Software-Updates via Windows Package Manager (WinGet).

## 🚀 Projekthintergrund & Lernkurve

Dieses Tool ist mehr als nur ein Utility-Programm – es ist mein praktisches Einstiegsprojekt in die moderne .NET-Entwicklung. 

Vor rund 15 Jahren habe ich meine ersten programmiertechnischen Grundlagen mit Visual Basic gelegt. Mein Ziel war es nun, dieses fundamentale Verständnis für Programm-Logik zu reaktivieren und direkt auf C# und das aktuelle .NET-Ökosystem zu übertragen. 

Anstatt nur trockene Theorie zu lesen, habe ich mich für einen modernen, praxisnahen Ansatz entschieden: Ich habe aktuelle KI-Modelle als "Pair Programmer" und interaktive Tutoren genutzt. Durch gezieltes Prompting, Fehleranalyse und Code-Reviews mit der KI konnte ich mich innerhalb kürzester Zeit in komplexe Konzepte wie asynchrone Programmierung, WMI-Schnittstellen und Custom-XAML/WPF einarbeiten.
## ✨ Kern-Features

* **Hardware-Analyse:** Detailliertes Auslesen und Strukturierung der Systemkomponenten (CPU, GPU, RAM, Mainboard, Netzwerk) über Windows System-APIs.
* **Echtzeit-Monitoring:** Asynchrones Live-Tracking der CPU-Auslastung und des Arbeitsspeichers für sofortiges visuelles Feedback, ohne das UI zu blockieren.
* **Speicher-Visualisierung:** Grafische Aufbereitung der Laufwerksbelegung (Kapazität vs. Nutzung) mittels intuitiver Fortschrittsbalken.
* **Modernes UI/UX:** Komplett eigenständiges Custom-WPF-Design (Dark Mode, abgerundete Ecken, Window-Styling) – umgesetzt ohne externe Drittanbieter-Frameworks.

## 🚧 Roadmap & Known Issues (WinGet)

Das Modul zur Software-Aktualisierung nutzt die WinGet-CLI im Hintergrund. Da sich WinGet noch in stetiger Entwicklung befindet, stehen hier folgende Optimierungen auf der Roadmap:
* **Parsing-Optimierung:** Verbesserung der Versionserkennung bei inkonsistenten Rückgaben der Paketquellen.
* **Fehlerbehandlung:** Besseres Handling von Timeouts und stabileres Verhalten, wenn die WinGet-Server zeitweise nicht erreichbar sind.

## 🛠️ Technik-Stack

* **Sprache:** C#
* **Framework:** .NET 9.0 (Windows Presentation Foundation)
* **Architektur:** Nutzung des neuen `.slnx` Solution-Formats
* **APIs:** WMI (Windows Management Instrumentation), WinGet CLI

## 📝 Lizenz

Dieses Projekt steht unter der [MIT-Lizenz](LICENSE). Reinschauen, lernen, forken und umbauen – alles ist ausdrücklich erlaubt!

---
Entwickelt von denneonblue – Ein bestehendes Logik-Fundament, erfolgreich überführt in eine moderne C#-Anwendung.
