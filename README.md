# Factory Queue System

## Local network development

Backend:

```powershell
dotnet run --urls "http://0.0.0.0:5141" --project backend\FactoryQueueSystem.Api\FactoryQueueSystem.Api.csproj
```

Flutter real Android device:

```powershell
flutter run --dart-define=API_BASE_URL=http://YOUR_PC_IP:5141
```

Flutter build APK:

```powershell
flutter build apk --dart-define=API_BASE_URL=http://YOUR_PC_IP:5141
```

The PC and phone must be connected to the same Wi-Fi network. Windows Firewall may ask for permission when the backend starts; allow access for the local network. Replace `YOUR_PC_IP` with the PC's IPv4 address from `ipconfig`.

Defaults:

- Flutter Web uses `http://localhost:5141`.
- Android emulator uses `http://10.0.2.2:5141`.
- Real Android devices should use `--dart-define=API_BASE_URL=http://YOUR_PC_IP:5141`.
