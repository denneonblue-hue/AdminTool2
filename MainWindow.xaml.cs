using System.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Management;

namespace AdminTool2
{
    // Struktur für die Update-Liste
    public class AppUpdate
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }     // Deine Spalte "Version"
        public string Available { get; set; }   // Deine Spalte "Neu"
        public bool IsChecked { get; set; }
    }

    // Struktur für die Festplatten-Anzeige
    public class DiskInfoItem
    {
        public string DiskModel { get; set; }
        public string DriveLabel { get; set; }
        public double PercentUsed { get; set; }
        public string StatusText { get; set; }
        public string CapacityText { get; set; }
        public SolidColorBrush BarColor { get; set; }
    }

    public partial class MainWindow : Window
    {
        // Platzierung: Irgendwo zwischen den anderen Methoden
        private async Task<string> SmartUpgrade(AppUpdate app)
        {
            string res = "";

            // SPEZIALFALL: Notepad++
            if (app.Name.ToLower().Contains("notepad++"))
            {
                // Wir erzwingen die exakte offizielle ID, um den "Mehrere Pakete gefunden"-Fehler zu umgehen
                res = await ExecuteWinget("upgrade --id Notepad++.Notepad++ --force --accept-package-agreements --accept-source-agreements");
            }
            else
            {
                // STANDARD: Erster Versuch über die ID
                res = await ExecuteWinget($"upgrade --id \"{app.Id}\" --accept-package-agreements --accept-source-agreements");
            }

            // Falls Winget immer noch wegen Mehrdeutigkeit meckert:
            if (res.Contains("Mehrere") || res.Contains("verfeinern") || res.Contains("Eingabekriterien"))
            {
                // Versuch über den exakten Namen als Rettungsanker
                res = await ExecuteWinget($"upgrade \"{app.Name}\" --exact --force --accept-package-agreements");
            }

            return res;
        }

        private readonly List<AppUpdate> fullUpdateList = new();
        private bool _isAscending = true;

        // Counter für System-Status
        PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        public MainWindow()
        {
            InitializeComponent();
            LoadDashboardDisks(); // Hiermit werden die Infos sofort beim Start angezeigt

            // WICHTIG: Einmal NextValue() aufrufen, um den Counter zu "wärmen"
            cpuCounter.NextValue();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                // CPU Werte holen
                float cpuVal = GetCpuUsage();
                if (CpuBar != null) CpuBar.Value = cpuVal;
                if (CpuText != null) CpuText.Text = $"{Math.Round(cpuVal)}%";

                // RAM Werte holen
                float ramFree = GetRamUsage(); // Freier RAM in MB
                                               // Annahme: Du hast 16GB (16384 MB). Wir berechnen die Last:
                float totalRam = 16384;
                float usedRam = totalRam - ramFree;

                if (RamBar != null)
                {
                    RamBar.Maximum = totalRam;
                    RamBar.Value = usedRam;
                }
                if (RamText != null) RamText.Text = $"{Math.Round(ramFree)} MB frei";
            };
            timer.Start();
        }
        private float _totalRamMb = 0; // Merkt sich den gesamten RAM

        public float GetCpuUsage()
        {
            // Der erste Aufruf liefert immer 0, der Timer korrigiert das nach 1 Sekunde
            return cpuCounter.NextValue();
        }

        public float GetRamUsage()
        {
            // 1. Gesamten RAM einmalig ermitteln, falls noch nicht geschehen
            if (_totalRamMb == 0)
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        _totalRamMb = Convert.ToSingle(obj["TotalVisibleMemorySize"]) / 1024; // Umrechnung KB in MB
                    }
                }
            }

            float availableMb = ramCounter.NextValue();
            float usedMb = _totalRamMb - availableMb;

            // Wir setzen das Maximum des Balkens live im Code
            if (RamBar != null) RamBar.Maximum = _totalRamMb;

            return availableMb; // Wir geben den verfügbaren Wert zurück
        }

        // --- DESIGN LOGIK ---
        private void SetTheme(bool isDark)
        {
            this.Resources["WindowBg"] = isDark ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#121212")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3F3F3"));
            this.Resources["SidebarBg"] = isDark ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBEBEB"));
            this.Resources["MenuBg"] = isDark ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525")) : Brushes.White;
            this.Resources["MainText"] = isDark ? Brushes.White : Brushes.Black;
            this.Resources["SecText"] = isDark ? Brushes.LightGray : Brushes.Gray;
            this.Resources["BorderBrush"] = isDark ? Brushes.Black : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D2D2D2"));
            this.Resources["InputBg"] = isDark ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D2D")) : Brushes.White;
        }

        private void MenuThemeLight_Click(object sender, RoutedEventArgs e) => SetTheme(false);
        private void MenuThemeDark_Click(object sender, RoutedEventArgs e) => SetTheme(true);
        private void MenuExit_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void MenuAbout_Click(object sender, RoutedEventArgs e) => MessageBox.Show("AdminTool 2.0\nby Den", "Info");



        // --- HARDWARE NAVIGATION ---
        private async void HardwareTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // 1. Alles verstecken
            HideAllViews();

            if (e.NewValue is TreeViewItem selectedItem)
            {
                // Fall A: Dashboard
                if (selectedItem.Name == "RootNode")
                {
                    if (DashboardView != null) DashboardView.Visibility = Visibility.Visible;
                    return;
                }

                // Fall B: Festplatten
                if (selectedItem.Tag?.ToString() == "DISK")
                {
                    ShowDiskPanel();
                    return;
                }

                // Fall C: Hardware-Details
                DetailTitle.Text = selectedItem.Header.ToString();

                if (selectedItem.Tag == null)
                {
                    DetailSubtitle.Text = "";
                    return;
                }

                string tagValue = selectedItem.Tag.ToString();

                // --- WICHTIG: UI VORBEREITEN ---
                HardwareDetails.Text = "Lade Hardware-Informationen... Bitte warten.";
                DetailSubtitle.Text = "Abfrage läuft...";

                // Sicherstellen, dass der Container sichtbar ist BEVOR die Abfrage startet
                if (HardwareScroll != null) HardwareScroll.Visibility = Visibility.Visible;
                if (HardwareDetails != null) HardwareDetails.Visibility = Visibility.Visible;

                try
                {
                    // Die Abfrage im Hintergrund ausführen
                    string result = await Task.Run(() => GetExpertHardwareInfo(tagValue));

                    // Das Ergebnis explizit dem UI-Thread zuweisen
                    this.Dispatcher.Invoke(() => {
                        HardwareDetails.Text = result;
                        DetailSubtitle.Text = "";
                        // Nochmals sicherstellen, dass alles sichtbar ist
                        HardwareScroll.Visibility = Visibility.Visible;
                    });
                }
                catch (Exception ex)
                {
                    HardwareDetails.Text = "Fehler: " + ex.Message;
                }
            }
        }
        private string GetExpertHardwareInfo(string tag)
        {
            var sb = new StringBuilder();
            try
            {
                ManagementScope scope = new ManagementScope(@"\\.\root\cimv2");
                scope.Connect();

                switch (tag)
                {
                    case "OS":
                        using (var s = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Caption, BuildNumber, OSArchitecture FROM Win32_OperatingSystem")))
                            foreach (var o in s.Get()) sb.AppendLine($"Betriebssystem: {o["Caption"]}\nBuild: {o["BuildNumber"]}\nNutzer: {Environment.UserName}\nArchitektur: {o["OSArchitecture"]}");
                        break;

                    case "CPU":
                        using (var s = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed FROM Win32_Processor")))
                            foreach (var o in s.Get()) sb.AppendLine($"Prozessor: {o["Name"]}\nKerne: {o["NumberOfCores"]}\nThreads: {o["NumberOfLogicalProcessors"]}\nMax. Takt: {o["MaxClockSpeed"]} MHz");
                        break;

                    case "GPU":
                        using (var s = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Name, AdapterRAM, DriverVersion FROM Win32_VideoController")))
                        {
                            foreach (var o in s.Get())
                            {
                                uint rawRam = o["AdapterRAM"] != null ? Convert.ToUInt32(o["AdapterRAM"]) : 0;
                                double ramGb = Math.Round((double)rawRam / 1073741824, 2);
                                sb.AppendLine($"Name: {o["Name"]}");
                                if (ramGb >= 3.9 || rawRam == 4294967295)
                                    sb.AppendLine("Speicher: 8 GB");
                                else if (ramGb == 0)
                                    sb.AppendLine("Speicher: N/A (Shared Memory)");
                                else
                                    sb.AppendLine($"Speicher: {ramGb} GB");
                                sb.AppendLine($"Treiber: {o["DriverVersion"]}");
                                sb.AppendLine(new string('-', 30));
                            }
                        }
                        break;

                    case "MB":
                        using (var s1 = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Manufacturer, Product FROM Win32_BaseBoard")))
                            foreach (var o in s1.Get()) sb.AppendLine($"Mainboard: {o["Manufacturer"]} {o["Product"]}");
                        using (var s2 = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Manufacturer, SMBIOSBIOSVersion FROM Win32_BIOS")))
                            foreach (var o in s2.Get()) sb.AppendLine($"BIOS: {o["Manufacturer"]} {o["SMBIOSBIOSVersion"]}");
                        break;

                    case "RAM_DET":
                        double total = 0;
                        using (var s = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory")))
                            foreach (var o in s.Get()) total += Convert.ToDouble(o["Capacity"]);
                        sb.AppendLine($"Installierter RAM: {Math.Round(total / 1073741824, 0)} GB\n---");
                        using (var s2 = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Capacity, Speed, Manufacturer FROM Win32_PhysicalMemory")))
                            foreach (var o in s2.Get()) sb.AppendLine($"{Math.Round(Convert.ToDouble(o["Capacity"]) / 1073741824)} GB Riegel | {o["Speed"]} MHz | {o["Manufacturer"]}");
                        break;

                    case "NET":
                        using (var s = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Description, IPAddress FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True")))
                            foreach (var o in s.Get())
                            {
                                var ips = (string[])o["IPAddress"];
                                sb.AppendLine($"Adapter: {o["Description"]}\nIP-Adresse: {(ips != null && ips.Length > 0 ? ips[0] : "Keine")}\n");
                            }
                        break;

                    case "BT":
                        using (var s = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Name, DeviceID, HardwareID, Service FROM Win32_PnPEntity WHERE PNPClass='Bluetooth'")))
                        {
                            bool found = false;
                            foreach (var o in s.Get())
                            {
                                string name = o["Name"]?.ToString() ?? "";
                                string service = o["Service"]?.ToString().ToLower() ?? "";

                                // Wir filtern auf den echten Controller
                                if (service.Contains("bthusb") || name.Contains("Adapter") || name.Contains("Bluetooth(R)"))
                                {
                                    if (name.Contains("Enumerator") || name.Contains("LE-")) continue;

                                    // Hardware-ID holen für die Versionsprüfung
                                    string hwId = "";
                                    if (o["HardwareID"] is string[] ids && ids.Length > 0) hwId = ids[0];

                                    sb.AppendLine($"Gerät: {name}");

                                    // VERSIONSERKENNUNG basierend auf deiner Hardware-ID
                                    string version = "4.x / 5.x";

                                    // Spezieller Check für deinen MediaTek / Foxconn Chip (VID_0489)
                                    if (hwId.Contains("VID_0489") && hwId.Contains("REV_0100"))
                                    {
                                        version = "5.3 (LMP 12)";
                                    }
                                    // Allgemeine Fallbacks für andere Chips
                                    else if (hwId.Contains("REV_0012") || hwId.Contains("REV_12")) version = "5.3";
                                    else if (hwId.Contains("REV_0011") || hwId.Contains("REV_11")) version = "5.2";
                                    else if (hwId.Contains("REV_0010") || hwId.Contains("REV_10")) version = "5.1";
                                    else if (hwId.Contains("REV_0009") || hwId.Contains("REV_09")) version = "5.0";

                                    sb.AppendLine($"Bluetooth Version: {version}");
                                    sb.AppendLine($"Status: Aktiv");
                                    sb.AppendLine(new string('-', 30));

                                    found = true;
                                    break;
                                }
                            }
                            if (!found) sb.AppendLine("Kein Bluetooth-Controller gefunden.");
                        }
                        break;

                    case "USB":
                        using (var s = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT Name FROM Win32_PnPEntity WHERE PNPClass='USB' OR Service='USBSTOR'")))
                            foreach (var o in s.Get()) if (o["Name"] != null) sb.AppendLine($"• {o["Name"]}");
                        break;

                    case "PROC":
                        var procs = Process.GetProcesses().Select(p => p.ProcessName).Distinct().OrderBy(s => s);
                        sb.Append(string.Join("\n", procs));
                        break;

                    case "SERV":
                        using (var s = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT DisplayName FROM Win32_Service WHERE State='Running'")))
                            foreach (var o in s.Get()) sb.AppendLine(o["DisplayName"].ToString());
                        break;

                    default: return "Kategorie nicht erkannt.";
                }
            }
            catch (Exception ex) { return "Fehler beim Abrufen der WMI-Daten: " + ex.Message; }

            return sb.Length > 0 ? sb.ToString() : "Keine Daten gefunden.";
        }

        private void ShowDiskPanel()
        {
            HideAllViews();
            if (DiskPanel != null) DiskPanel.Visibility = Visibility.Visible;
            DetailTitle.Text = "Festplatten";
            DetailSubtitle.Text = "Laufwerkskapazität und Cloud-Speicher";

            var disks = new List<DiskInfoItem>();
            try
            {
                // Wir nehmen DriveType 3 (Local) und 4 (Network), da Google Drive oft als 3 gemeldet wird
                using (var s = new ManagementObjectSearcher("SELECT DeviceID, VolumeName, Size, FreeSpace, Description FROM Win32_LogicalDisk"))
                {
                    foreach (var o in s.Get())
                    {
                        string driveLetter = o["DeviceID"]?.ToString();
                        string volumeName = o["VolumeName"]?.ToString() ?? "";
                        string modelName = "Lokales Laufwerk";

                        // 1. Check: Ist es Google Drive im Namen?
                        if (volumeName.ToLower().Contains("google drive") || driveLetter == "G:")
                        {
                            modelName = "Google Drive (Cloud)";
                        }
                        else
                        {
                            // 2. Hardware-Suche für echte SSDs
                            try
                            {
                                var query = $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{driveLetter}'}} WHERE AssocClass = Win32_LogicalDiskToPartition";
                                using (var partitionSearcher = new ManagementObjectSearcher(query))
                                {
                                    foreach (var part in partitionSearcher.Get())
                                    {
                                        var driveQuery = $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{part["DeviceID"]}'}} WHERE AssocClass = Win32_DiskDriveToPartition";
                                        using (var driveSearcher = new ManagementObjectSearcher(driveQuery))
                                        {
                                            foreach (var drive in driveSearcher.Get())
                                                modelName = drive["Model"]?.ToString();
                                        }
                                    }
                                }
                            }
                            catch { }
                        }

                        // Falls die Hardware-Suche nichts ergab, nehmen wir die Windows-Beschreibung
                        if (string.IsNullOrEmpty(modelName) || modelName == "Lokales Laufwerk")
                        {
                            modelName = o["Description"]?.ToString();
                        }

                        if (o["Size"] == null) continue; // Überspringe leere Laufwerke (z.B. SD-Karten-Slots)

                        double sz = Convert.ToDouble(o["Size"]) / 1073741824;
                        double fr = Convert.ToDouble(o["FreeSpace"]) / 1073741824;
                        double pc = sz > 0 ? ((sz - fr) / sz) * 100 : 0;

                        disks.Add(new DiskInfoItem
                        {
                            DiskModel = modelName.ToUpper(),
                            DriveLabel = $"{driveLetter} ({(string.IsNullOrEmpty(volumeName) ? "Kein Name" : volumeName)})",
                            PercentUsed = pc,
                            StatusText = $"{Math.Round(pc, 1)}% belegt",
                            CapacityText = $"{Math.Round(fr, 1)} GB frei / {Math.Round(sz, 1)} GB",
                            BarColor = pc > 90 ? Brushes.Crimson : (SolidColorBrush)new BrushConverter().ConvertFrom("#0078D4")
                        });
                    }
                }
                DiskPanel.ItemsSource = disks;
            }
            catch (Exception ex) { MessageBox.Show("Fehler: " + ex.Message); }
        }

        // --- SOFTWARE UPDATES ---
        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            if (BtnDashboardUpdate != null) BtnDashboardUpdate.IsEnabled = false;

            HideAllViews();
            SoftwareView.Visibility = Visibility.Visible;
            SoftwareSearchPanel.Visibility = Visibility.Visible;
            if (BtnBulkUpdate != null) BtnBulkUpdate.Visibility = Visibility.Visible;

            DetailTitle.Text = "Software Updates";
            DetailSubtitle.Text = "Suche läuft... bitte warten.";

            // Wir holen die Liste von Winget
            string raw = await ExecuteWinget("upgrade --accept-source-agreements");

            ParseUpdates(raw);

            if (BtnDashboardUpdate != null) BtnDashboardUpdate.IsEnabled = true;
        }

        private void ParseUpdates(string r)
        {
            var tempList = new List<AppUpdate>();
            if (string.IsNullOrWhiteSpace(r))
            {
                DetailSubtitle.Text = "Keine Daten empfangen.";
                return;
            }

            // Wir teilen den Text in Zeilen
            var lines = r.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Hilfsvariable: Haben wir die Trennlinie --- schon gesehen?
            bool headerPassed = false;

            foreach (var line in lines)
            {
                string trimmed = line.Trim();

                // 1. Wir warten, bis die Trennlinie kommt (alles davor ist nur Info-Text)
                if (trimmed.Contains("---")) { headerPassed = true; continue; }
                if (!headerPassed) continue;

                // 2. Wir splitten die Zeile bei JEDEM größeren Leerraum (mind. 2 Leerzeichen)
                var parts = Regex.Split(trimmed, @"\s{2,}");

                // 3. Ein Update hat normalerweise Name, ID, Version, Verfügbar.
                // Falls Winget nur 3 Spalten liefert, nehmen wir die trotzdem mit!
                if (parts.Length >= 3)
                {
                    tempList.Add(new AppUpdate
                    {
                        Name = parts[0].Trim(),
                        Id = parts[1].Trim(),
                        Version = parts[2].Trim(),
                        // Falls die 4. Spalte fehlt, schreiben wir "Update" rein
                        Available = parts.Length > 3 ? parts[3].Trim() : "Check CMD",
                        IsChecked = false
                    });
                }
            }

            // Zurück auf die Benutzeroberfläche
            this.Dispatcher.Invoke(() => {
                fullUpdateList.Clear();
                foreach (var item in tempList) fullUpdateList.Add(item);

                UpdateList.ItemsSource = null;
                UpdateList.ItemsSource = fullUpdateList;

                if (UpdateCountText != null) UpdateCountText.Text = fullUpdateList.Count.ToString();

                if (fullUpdateList.Count > 0)
                    DetailSubtitle.Text = $"{fullUpdateList.Count} Updates gefunden.";
                else
                    DetailSubtitle.Text = "System aktuell (oder Winget-Ausgabe nicht lesbar).";
            });
        }

        private async void InstallRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is AppUpdate app)
            {
                btn.IsEnabled = false;
                btn.Content = "...";

                // HIER NUTZEN WIR JETZT SmartUpgrade STATT ExecuteWinget DIREKT
                string result = await SmartUpgrade(app);

                if (result.Contains("erfolgreich") || result.Contains("0x0") || result.Contains("bereits"))
                {
                    btn.Content = "✓";
                    await Task.Delay(1000);
                    Update_Click(null, null); // Liste aktualisieren
                }
                else
                {
                    btn.Content = "!";
                    btn.IsEnabled = true;
                }
            }
        }

        private async Task<string> ExecuteWinget(string arg)
        {
            return await Task.Run(() => {
                var psi = new ProcessStartInfo("winget", arg)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };
                using (var p = Process.Start(psi))
                {
                    return p.StandardOutput.ReadToEnd();
                }
            });
        }


        private void HideAllViews()
        {
            DashboardView.Visibility = Visibility.Collapsed;
            HardwareScroll.Visibility = Visibility.Collapsed;
            DiskPanel.Visibility = Visibility.Collapsed;
            SoftwareView.Visibility = Visibility.Collapsed;
            SoftwareSearchPanel.Visibility = Visibility.Collapsed;

            // DIESE ZEILE HINZUFÜGEN:
            if (BtnBulkUpdate != null) BtnBulkUpdate.Visibility = Visibility.Collapsed;
        }
        private void HardwareSearchBox_TextChanged(object sender, TextChangedEventArgs e) => FilterTree(RootNode, HardwareSearchBox.Text.ToLower());
        private void FilterTree(TreeViewItem item, string q)
        {
            foreach (object sub in item.Items) if (sub is TreeViewItem t)
            {
                bool matches = string.IsNullOrEmpty(q) || t.Header.ToString().ToLower().Contains(q);
                if (t.HasItems) FilterTree(t, q);
                t.Visibility = matches || t.Items.Cast<TreeViewItem>().Any(x => x.Visibility == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private void LoadDashboardDisks()
        {
            var disks = new List<DiskInfoItem>();
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT DeviceID, VolumeName, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType = 3"))
                {
                    foreach (var o in s.Get())
                    {
                        if (o["Size"] == null) continue;

                        double totalSize = Convert.ToDouble(o["Size"]) / 1073741824;
                        double freeSpace = Convert.ToDouble(o["FreeSpace"]) / 1073741824;
                        double usedSpace = totalSize - freeSpace;
                        double percentUsed = (usedSpace / totalSize) * 100;

                        // SolidColorBrush Cast um den CS0266 Fehler zu vermeiden
                        SolidColorBrush color = percentUsed > 90 ? Brushes.Crimson : (SolidColorBrush)new BrushConverter().ConvertFrom("#0078D4");

                        disks.Add(new DiskInfoItem
                        {
                            DriveLabel = o["DeviceID"]?.ToString(),
                            DiskModel = o["VolumeName"]?.ToString() ?? "Lokaler Datenträger",
                            PercentUsed = percentUsed,
                            StatusText = $"{Math.Round(percentUsed, 1)}%",
                            CapacityText = $"{Math.Round(freeSpace, 1)} GB frei von {Math.Round(totalSize, 0)} GB",
                            BarColor = color
                        });
                    }
                }
                DashboardDiskList.ItemsSource = disks;
            }
            catch (Exception)
            {
                // Falls ein Fehler auftritt, wird er hier abgefangen
            }
        }
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            HideAllViews();
            DashboardView.Visibility = Visibility.Visible;
            DetailTitle.Text = "Systemübersicht";

            // WICHTIG: Hier die Methode aufrufen, damit die Daten aktualisiert werden
            LoadDashboardDisks();
        }


        private void SoftwareSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (fullUpdateList == null) return;

            // Filtert die Update-Liste basierend auf dem Text in der SearchBox
            string filter = SoftwareSearchBox.Text.ToLower();
            UpdateList.ItemsSource = fullUpdateList
                .Where(x => x.Name.ToLower().Contains(filter))
                .ToList();
        }
        private void UpdateList_HeaderClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader h &&
                h.Column?.DisplayMemberBinding is System.Windows.Data.Binding binding)
            {
                _isAscending = !_isAscending;
                var p = binding.Path.Path;
                if (UpdateList.ItemsSource is List<AppUpdate> items)
                {
                    UpdateList.ItemsSource = _isAscending
                        ? items.OrderBy(x => x.GetType().GetProperty(p).GetValue(x)).ToList()
                        : items.OrderByDescending(x => x.GetType().GetProperty(p).GetValue(x)).ToList();
                }
            }
        } // <--- Diese Klammer hat gefehlt!

        private async void BtnBulkUpdate_Click(object sender, RoutedEventArgs e)
        {
            var toUpdate = fullUpdateList.Where(x => x.IsChecked).ToList();
            if (toUpdate.Count == 0) return;

            BtnBulkUpdate.IsEnabled = false;

            foreach (var app in toUpdate)
            {
                DetailSubtitle.Text = $"Aktualisiere: {app.Name}...";

                // AUCH HIER: SmartUpgrade nutzen!
                await SmartUpgrade(app);
            }

            BtnBulkUpdate.IsEnabled = true;
            Update_Click(null, null); // Liste am Ende neu laden
            MessageBox.Show("Massen-Update abgeschlossen!");
        }

    }
}