# M5 — lượt dựng hình đầu tiên

## CafeManagerMorning — quản lý dạng người (mới nhất)

Menu `Qua Thang Nay Da > M5 > Open Cafe Manager Morning (latest)`.
Scene `Assets/_Project/Scenes/CafeManagerMorning.unity`; bản Windows M5 dùng scene này.
Giữ scene trước để đối chiếu. Capsule quản lý cũ được tắt trong scene mới.

Quản lý dùng cùng bộ hình khối nhân vật tự dựng: tóc ngắn, áo xanh xám có cổ,
tạp dề nâu, bảng tên, tỉ lệ hơi cao/rộng hơn người bán xôi; đứng hướng ra quán.
Tương tác trên các bộ phận nhận chung CoverShift, không chuyển sang Breakfast.
Không sửa luật lời mời, hạn thực hiện, thưởng/phạt; chưa có animation hoặc thoại mới.

Kiểm tra batch: đúng một NPC CoverShift hoạt động, ray đầu/thân nhận đúng NPC,
người bán xôi vẫn là Breakfast. Cần Play kiểm tra ngoại hình, khoảng đứng, F và cam kết.

## BikeVendorMorning — xe đỏ đen và người bán (mới nhất)

Menu `Qua Thang Nay Da > M5 > Open Bike and Vendor Morning (latest)`.
Scene `Assets/_Project/Scenes/BikeVendorMorning.unity`; build Windows M5 dùng scene này.

- Xe máy dạng xe số đỏ–đen theo dáng tham khảo: bánh/vành, thân, yên, động cơ,
  chắn bùn, đầu đèn, tay lái, gương, ống pô. Tự dựng hình khối, không nhãn hiệu;
  chưa phải model chi tiết tương đương ảnh, bánh chưa có animation quay.
- Visual không có collider; giữ Rigidbody/BoxCollider gốc, Seat và SafeSpot.
  Không đổi tốc độ, phanh, hồi phục xe hoặc lưu/tải.
- Người bán đứng cạnh xe xôi, hình người stylized có tạp dề, khuôn mặt đơn giản.
  F trên người bán cũng mở hoạt động ăn sáng với cùng luật, không nhận phần thưởng hai lần.
  NPC tĩnh: chưa có hội thoại hoặc animation phục vụ.
- Scene cũ được giữ nguyên. Không ghi đè tiến trình M4.

Batch xác nhận một collider gốc trên xe, tham chiếu Seat/SafeSpot, ray tương tác người bán.
Cần kiểm tra Play: hình dáng, gương có chắn góc nhìn không, đi quanh người bán,
lên/xuống, chạy/phanh/va chạm, mua xôi qua NPC và xe hàng, lưu/tải.
Chưa xác minh trực tiếp các mục này hay FPS.

## StreetMorning — xe xôi và tương tác nguyên đồ vật (mới nhất)

Menu `Qua Thang Nay Da > M5 > Open Street Morning - Xoi Cart (latest)`.
Scene `Assets/_Project/Scenes/StreetMorning.unity`; bản Windows M5 dùng scene này.
Giữ nguyên CampusMorning để đối chiếu, không tự ghi đè scene người dùng.

- Khung giường, nệm, chăn, gối có collider và cùng trỏ về một ActivityStation ở gốc giường.
- Quầy và mặt quầy cùng một chủ tương tác; không gán hoạt động cho cả căn phòng.
- Toàn bộ phần hình học xe xôi dùng chung hoạt động ăn sáng. Xe ở (-4.5,0,-2.5),
  ngoài phố gần phòng trọ; biển đỏ, khung kim loại, bánh và nồi xôi tự dựng từ hình khối.
- Bỏ hoạt động ăn sáng ở bàn/khay cũ; ẩn khay và bánh cũ, giữ bàn/laptop/cốc.
- Giá, thời gian, tác động chỉ số ăn sáng không đổi. Chưa có người bán/hoạt ảnh phục vụ.
- Không sử dụng số điện thoại, logo hay ảnh quảng cáo từ hình tham khảo làm texture.

Kiểm tra batch: sở hữu tương tác của mọi bộ phận giường/xe, ray từ trên xuống giường,
ray tới thân/biển/bánh xe và xác nhận lại; chỉ còn xe xôi là điểm ăn sáng hoạt động.
Cần Play kiểm tra hình ảnh, F ở nhiều góc, đi quanh xe và lưu/tải trong cảnh mới.

## CampusMorning — bản mới nhất

### Sửa khoảng tương tác 2026-11-05

Giới hạn 3 m đo từ camera tới bề mặt collider, không phải tâm đồ vật.
Sau phản hồi chọn sai vật, đã BỎ tìm vật gần thay thế. Chỉ nhận collider ngay dưới tâm,
không chọn vật bên cạnh khi tia hụt hoặc gặp vật không tương tác. Bàn/giường thấp cần nhìn xuống.
Bắt mục tiêu và F trong LateUpdate sau camera; ẩn ngay chữ cũ khi rời mục tiêu.
Không chọn vật phía sau hoặc xuyên tường. Hiện gợi ý và xác nhận dùng cùng giới hạn.
Kiểm tra batch: 0.05, 0.25, 0.5, 1, 2, 2.99, 3 m; loại 3.1 m; bàn thấp,
quay lưng, vật cản, nhiều mục tiêu và vật gần hơn nằm lệch tâm.
Vẫn cần thử cảm giác chọn mục tiêu giữa nhiều đồ vật trong Play.

Mở `Qua Thang Nay Da > M5 > Open Campus Morning (latest)` sau khi dừng Play.
Scene `Assets/_Project/Scenes/CampusMorning.unity`; build Windows M5 dùng scene này.
Các scene trước được giữ để đối chiếu, không tự nhận cập nhật hình học mới.

- Giường: cả khung và nệm nhận F.
- Ăn sáng: khay vàng lớn hơn trên bàn phòng trọ; cả mặt bàn cũng nhận F.
- Chỉ hiện gợi ý [F] nhỏ khi ngắm trúng vật trong 3 m; mờ dần khi rời mục tiêu.
  Không hiện gợi ý khi tạm dừng, mở điện thoại, đọc tình huống hoặc đang hoạt động.
- Lớp học ở tòa bên trái kế tiếp phòng trọ (x=-10,z=4), có cửa và lối giữa,
  bốn bàn học, ghế và bảng. Nhìn bàn/mặt ghế/lưng ghế rồi F để mở xác nhận học.
  Luật giờ học vẫn giữ nguyên; chưa có hoạt ảnh ngồi hay giáo viên.
- Sau F vẫn cần xác nhận bắt đầu trong bảng hoạt động. Esc tiếp tục nếu đang tạm dừng.

Kiểm tra batch thêm tia nhìn thực tế tới khung/nệm, khay ăn, quầy, bàn học;
kiểm tra capsule lối vào và lối giữa lớp. Không chỉ kiểm tra sự tồn tại của collider.
Cần thử trực tiếp: F từ nhiều góc, gợi ý hiện/ẩn qua cửa/tường, chữ và khay nhìn rõ,
đi học đúng giờ, hai nhánh lời mời, lưu/tải. Chưa xác nhận hình ảnh hoặc FPS.

## Bản chi tiết mới

Menu `Qua Thang Nay Da > M5 > Open Detailed Morning (latest)` mở
`Assets/_Project/Scenes/ArtMorningDetail.unity`. Build Windows M5 hiện dùng scene này.
Scene ArtMorning cũ được giữ nguyên để đối chiếu, không tự ghi đè.

Thêm cửa ra vào phòng trọ, cửa sổ trang trí, mái có gờ, vỉa hè, đèn treo,
tủ, khay ăn và đồ dùng trên bàn. Vật liệu vẫn là màu phẳng, chưa có texture thực tế.
Các pane cửa sổ là mặt đục trang trí, chưa có kính trong hoặc phản chiếu.

- Ăn sáng: nhìn khay trên bàn trong phòng trọ, F.
- Nghỉ: nhìn nệm giường, F.
- Làm ca thường: nhìn mặt trước hoặc mặt trên quầy cà phê, F.
- Làm thay: gặp NPC quản lý đã chuyển vào quán, F.
- Lớp học vẫn dùng trạm prototype ngoài phố.

Các cột tương ứng cũ được tắt trong scene mới, không xóa khỏi scene gốc.
Giữ chung các ô lưu thử mỹ thuật; không tải trạng thái giữa hai scene M5 để nghiệm thu
vì vị trí/trạm đã thay đổi. M4 vẫn dùng thư mục lưu riêng.

Kiểm tra tự động bổ sung: tham chiếu player/phone/save, collider của mọi loại hoạt động,
khoảng trống cửa bằng capsule kích thước người chơi. Chưa thay thế kiểm tra đi lại,
tia tương tác, hình ảnh và FPS trong bản chạy.

## Bản bố cục trước đó

Mở menu `Qua Thang Nay Da > M5 > Create or Open Art Morning`, hoặc scene
`Assets/_Project/Scenes/ArtMorning.unity`. Dừng Play trước khi đổi scene.

Scene riêng sao từ M4, không ghi đè M4. Phòng trọ ở bên trái gần đầu đường
(x=-10, z=-8), quán bên phải phía trước (x=10, z=16). Cửa mở hướng ra đường.
Điều khiển WASD/chuột, E lên xuống xe, F tương tác, P điện thoại vẫn giữ nguyên.
Các điểm hoạt động vẫn ở vị trí thử nghiệm cũ; đồ nội thất mới chỉ là trang trí.

Toàn bộ hình khối và vật liệu mới tự dựng bằng Unity, không nhập model chưa rõ bản quyền.
Đây là bản bố cục/màu đầu tiên, chưa phải chất lượng hình ảnh cuối cùng. Nhân vật, xe,
các tòa nhà còn lại và UI vẫn là prototype. Chưa có texture chi tiết, lightmap, animation mới.

M5 dùng `Saves-art-preview-v1` dưới persistentDataPath để không ghi đè các ô M4
trong `Saves-v1`. Không tự nhập các ô M4 sang bản đồ thử mỹ thuật.

## Cần kiểm tra trực tiếp

- Đi vào/ra cả hai khu mẫu, không kẹt cửa, không xuyên tường/nội thất lớn.
- Kiểm tra mặt biển hiệu, bóng đổ, mức sáng trong phòng, không vật liệu hồng.
- Đi xe qua mặt tiền, không mắc cây hoặc mái hiên; kiểm tra FPS ở 1080p.
- F ăn/học/làm, P tình huống; thử hai nhánh cam kết.
- Lưu trong phòng/quán, tải lại đúng vị trí; các ô M4 không bị thay đổi.

Build và các kiểm tra luật chạy trong cache riêng không thay thế kiểm tra hình ảnh,
va chạm trong bản chạy hoặc đo hiệu năng. Chưa nghiệm thu các mục trên.
