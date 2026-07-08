# Fabrika Sıra Sistemi Demo Akışı

## Demo Giriş Bilgileri

Admin:

- E-posta: `admin@factoryqueue.local`
- Şifre: `Admin123!`

Şoför:

- E-posta: `driver@factoryqueue.local`
- Şifre: `Driver123!`

## Final Demo Kontrol Listesi

1. EF Core migration'ı yapılandırılmış SQL Server veritabanına uygula.
2. Backend projesini başlat.
3. Mobil uygulamada demo şoför bilgileriyle giriş yap.
4. Aktif sevkiyatı aç.
5. Günlük sıra numarası almak için `Tesise Geldim` butonuna bas.
6. Demo admin bilgileriyle `/Admin/Login` sayfasından giriş yap.
7. `/Admin/Queue` sayfasını aç ve aracı kantara çağır.
8. `/Admin/Shipments` sayfasında dolu tartımı gir, boşaltımı başlat ve tamamla.
9. Boş tartımı gir.
10. Sevkiyatın `Tamamlandı` durumuna geçtiğini kontrol et.
11. `/Admin/Shipments/Completed` sayfasında dolu tartım, boş tartım ve net miktarı doğrula.
12. Mobil uygulamada durum ekranından sonuç ekranına geç.
13. Dolu tartım, boş tartım, net teslim miktarı ve tartım zamanlarının göründüğünü doğrula.
