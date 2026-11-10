# M6 — giao thông và bệnh viện

Mở `Qua Thang Nay Da > Open Main Morning - Traffic and Hospital`, hoặc
[MainMorning](../Assets/_Project/Scenes/MainMorning.unity). Dừng Play trước khi đổi scene.
Đây là scene chính từ mốc này: các menu tạo scene không ghi đè nếu scene đã tồn tại.
Các scene trước được giữ nguyên. Build: `Build/Windows-M6/QuaThangNayDa-M6.exe`.

## Cách chơi

- Sau nút thức dậy, 2 ô tô và 2 xe máy chạy hai làn ngược chiều, giảm tốc trước vật cản.
  Khi tới cuối đoạn đường, xe chỉ quay về đầu làn nếu đầu làn trống. Chưa có giao lộ/đèn giao thông.
- Va chạm giao thông xét tốc độ tương đối khi đi bộ hoặc lái xe. Không gây thương tích trong
  tutorial, pause, tình huống đang chờ lựa chọn hoặc đang điều trị. Một tiếp xúc không trừ dồn.
- Bệnh viện ở cuối phố phía bên phải (x=10,z=28). Tầng 1 tiếp nhận; cầu thang bên phải
  trong nhà lên tầng 2 có giường điều trị. Có thể đi tham quan hai tầng khi không nhập viện.
- Nhập viện tự đưa người chơi tới tiếp nhận/phòng bệnh; xe máy được đỗ ở ngoài bệnh viện.
- Nhẹ: bấm **Xuất viện** ngay. Nặng: bấm **Điều trị** để tiến tối đa 10 phút mỗi lần;
  dừng ở mốc lịch/cam kết/hoàn tất. Bấm **Xuất viện** khi đủ thời gian.
- Học/làm vẫn tính thiếu phút; cam kết vẫn hết hạn. Không trừ học tập riêng vì chấn thương.
- Thiếu tiền vẫn được chữa, phần thiếu ghi **Nợ viện phí**. Chưa có UI trả nợ ở mốc này.
- P vẫn mở điện thoại để lưu/tải khi nhập viện. Tải lại luôn tạm dừng, không thu viện phí lần nữa.

## Cân bằng tạm thời (số liệu gameplay, không phải mô phỏng y tế)

| Tốc độ tương đối | Hậu quả |
|---|---|
| Dưới 2 m/s | Không giảm sức khỏe |
| 2 đến dưới 5 m/s | Trừ 2 sức khỏe, không nhập viện |
| 5 đến dưới 10 m/s | Trừ 15 sức khỏe, viện phí 20.000đ, có thể xuất viện ngay |
| Từ 10 m/s | Trừ 40 sức khỏe, viện phí 60.000đ, điều trị 30 phút |

Chấn thương nhập viện giữ tối thiểu 1 sức khỏe để còn được chữa; xuất viện hồi phục ít nhất 60.
Ngưỡng, viện phí, thời gian và sức khỏe cấu hình trong MorningConfig.HospitalRules.
Phí chốt một lần lúc nhập viện: tiền có trước, phần thiếu thành nợ. Điều trị qua 12:00
vẫn phải hoàn thành, xuất viện rồi kết thúc buổi sáng; không mở rộng khung học/làm.

## Lưu và giới hạn

Ô lưu M6 tách vào `Saves-traffic-v1` dưới persistentDataPath. M4/M5 không bị ghi đè.
Lưu được trạng thái điều trị, phút còn lại, nợ, chỉ số và vị trí người/xe máy.
Xe giao thông là cảnh nền: vị trí của chúng chưa được lưu; khi tải chúng vẫn theo trạng thái
cảnh hiện tại hoặc vị trí khởi đầu nếu vừa mở game. Có bảo vệ va chạm 4 giây sau tải.
Giao thông dừng khi đang điều trị/tua giờ: xử lý thời gian bằng luật mô phỏng, không chạy
hàng nghìn bước vật lý. NPC xe/khối bệnh viện hiện là model prototype.

## Kiểm tra

- Core: trầy nhẹ/nhập viện nhẹ/nặng, viện phí một lần/nợ, pause, save-load, quá giờ kết thúc,
  chuyên cần/lương và deadline cam kết; giữ bộ kiểm tra các mốc trước.
- Unity batch: tham chiếu scene, điểm dịch chuyển không kẹt, 16 bậc có mặt đỡ,
  CharacterController thực sự leo lên/xuống cầu thang.
- Adapter batch: nhập viện người đi bộ/người lái, không thu phí lặp, đỗ xe,
  snapshot toàn game; Rigidbody giao thông chạy và dừng trước vật cản.

Chưa thay thế nghiệm thu Play. Cần thử trong bản chạy: tông từ nhiều hướng, tốc độ/âm báo,
đi hai tầng, UI điều trị/điện thoại, tải khi đang nằm viện, hai nhánh cam kết và FPS 1080p.
Chưa đo hiệu năng hoặc xác minh hình ảnh trực tiếp.
