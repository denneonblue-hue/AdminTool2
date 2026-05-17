# AdminTool2 – System Monitor & Update Manager

Ein modernes WPF-Tool zur Echtzeit-Überwachung von Hardware-Ressourcen und zur komfortablen Verwaltung von Software-Updates über winget.

<img width="1186" height="893" alt="light" src="https://github.com/user-attachments/assets/c453f9eb-35ca-4529-873e-8906a5ffd057" />
<img width="1186" height="893" alt="dark" src="https://github.com/user-attachments/assets/fa9a1e31-e3f8-4495-bfd8-2e2688e1eee5" />


## Projektbeschreibung
AdminTool2 ist ein eigenständig entwickeltes Dashboard, das Hardware-Auslastung live visualisiert und die Software-Verwaltung unter Windows stark vereinfacht. Das Tool entstand aus dem Wunsch, administrative Aufgaben effizienter zu gestalten – mit Fokus auf Performance, modernes Design und praktische Nutzbarkeit.

## Technischer Hintergrund & Stack
* Framework & Sprache: .NET 9.0 + WPF (C#) unter Nutzung des neuen .slnx-Solution-Formats
* APIs & Schnittstellen: Echtzeit-Hardware-Monitoring über WMI (Windows Management Instrumentation) und PerformanceCounter
* Architektur: Konsequente asynchrone Verarbeitung zur Vermeidung von UI-Blocking (Thread-Sicherheit)
* Integration: Windows Package Manager (winget CLI) zum Suchen, Installieren und Aktualisieren von Anwendungen
* UI/UX: Vollständig eigenes Custom UI mit Dark Mode und abgerundeten Elementen – komplett ohne externe Drittanbieter-UI-Frameworks umgesetzt

## Meine Motivation & Vorkenntnisse

### Das Fundament (Reverse Engineering & Serverstrukturen)
Bereits vor über 15 Jahren habe ich mir durch Reverse Engineering, Modifikation von Game-Clients (u. a. die eigenständige Entwicklung des NeonBlue Modified Clients für Metin2) sowie dem Aufsetzen und Verwalten eigener Linux-basierter Serverstrukturen und SQL-Datenbanken tiefe IT-Grundlagen angeeignet.

### Das Mindset (Aus der Präzisionsfertigung in den Code)
Beruflich komme ich aus der industriellen Präzisionsfertigung im $\mu$-Bereich. Diese Arbeitsweise prägt meinen Code: In der Fertigung führt ein falscher Parameter zu teurem Ausschuss – in der IT zu Systemfehlern. Ich entwickle mit demselben Fokus auf absolute Prozesssicherheit, Fehlervermeidung und logische Stringenz.

### Die Umsetzung (Moderne Entwicklung)
AdminTool2 ist mein Einstiegsprojekt in C#. Ich habe es bewusst als Lernprojekt mit hohem Praxisnutzen angelegt und dabei aktuelle KI-Modelle gezielt im Pair-Programming-Ansatz als interaktive Tutoren für Code-Reviews und effizientes Troubleshooting genutzt.

## Kernfunktionen
* Live-Überwachung: Echtzeit-Tracking von CPU, RAM und Systemkomponenten ohne UI-Verzögerung
* Hardware-Übersicht: Detaillierte, strukturierte Systemanalyse via TreeView-Komponenten
* Speicher-Visualisierung: Grafische Aufbereitung der Laufwerksbelegung mittels Fortschrittsbalken
* winget-Anbindung: Komfortables Suchen, Installieren und Deinstallieren von Windows-Software

## Installation & Nutzung

### Für Entwickler / Code-Review:
1. Repository lokal klonen: `git clone https://github.com/Denneonblue-hue/AdminTool2.git`
2. Die `AdminTool2.slnx` in Visual Studio 2022 öffnen.
3. Sicherstellen, dass das .NET 9.0 SDK installiert ist.
4. Per F5 kompilieren und starten.

### Für Endanwender:
* Die fertige, ausführbare Anwendung ist im Bereich Releases als .zip-Archiv verfügbar.
* Hinweis: Für den vollen Funktionsumfang des winget-Moduls wird das Ausführen mit Administratorrechten empfohlen.
* Systemanforderungen: Windows 10 / 11 (64-Bit)

---
Entwickelt von denneonblue – Ein bestehendes Logik-Fundament, erfolgreich überführt in eine moderne C#-Anwendung.
