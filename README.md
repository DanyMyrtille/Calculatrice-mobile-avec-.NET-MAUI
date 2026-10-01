# Calculatrice – .NET MAUI

Calculatrice mobile (+ − × ÷, décimaux, C, ←, ±, %, division par zéro gérée).
Layouts : Grid, Border, VerticalStackLayout, HorizontalStackLayout, ScrollView.

## Prérequis
- SDK .NET 10 : https://dotnet.microsoft.com/download
- Workload MAUI : `dotnet workload install maui` (`sudo` sur Mac)
- Mac : Xcode installé (pour Mac Catalyst / iOS)
- Android : Android Studio (SDK + émulateur)

## Lancer
```bash
dotnet restore

# Mac
dotnet build -f net10.0-maccatalyst -t:Run

# Windows
dotnet build -f net10.0-windows10.0.19041.0
dotnet run -f net10.0-windows10.0.19041.0

# Android (émulateur démarré ou téléphone branché)
dotnet build -f net10.0-android -t:Run
```

## APK
```bash
dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormat=apk
```
APK dans `bin/Release/net10.0-android/publish/`.

## Structure
- `MainPage.xaml` / `MainPage.xaml.cs` : interface, orientation, événements
- `CalculatorEngine.cs` : logique de calcul
- `Platforms/` : fichiers spécifiques Android, iOS, Mac Catalyst, Windows
