# M2 — lát cắt đầu: thời gian, lịch và sinh hoạt

## Mở và chơi

Tắt Play, chờ compile, chọn **Qua Thang Nay Da → M2 → Create or Open Morning Test**,
rồi Play. Scene riêng MorningTest giữ nguyên M0/M1. Hoặc mở
`Build/Windows-M2/QuaThangNayDa-M2.exe` sau build thành công.

1. Đọc hướng dẫn an toàn: giờ dừng ở 07:00 cho tới khi bấm Thức dậy.
2. Đi bộ tới bảng AN SANG gần xe, nhìn bảng trong khoảng 3 m và bấm F.
3. Đọc thời lượng/chi phí/cảnh báo rồi Xác nhận. Bấm thực hiện tiếp để tiến tối đa 10 phút.
4. Tab đóng/mở lịch và nhật ký. Bảng thông thường vẫn chạy giờ; Esc dừng cả game.
5. Để tới giờ học nhanh, có thể nghỉ tại bảng NGHI NGOI gần điểm xuất phát.
6. Tới LOP HOC bên trái khu phố lúc 08:00–09:00; tới CA PHE bên phải lúc 09:30–10:30.
   Phải xuống xe, đứng gần và nhìn bảng mới bắt đầu. Trong hoạt động không đi/lái xe.
7. Có thể dừng hoạt động giữa chừng rồi vào lại. Lương và học tập được chốt khi kết thúc khung lịch.
8. Đến 12:00 dừng lượt, hiện nhật ký tổng kết. Stop/Play để thử lại; CHƯA có save/load.

Các bảng thay cho bàn ăn, ghế lớp và NPC quản lý trong lát cắt kỹ thuật này.
Chưa phải tương tác/hoạt ảnh ngồi học hoặc làm cà phê hoàn chỉnh.

## Cấu hình thử — chưa chốt cân bằng

MorningConfig là ScriptableObject, lưu ở `Assets/_Project/Data/MorningConfig.asset`.
Lịch được sao chép khi bắt đầu lượt, thay đổi config không sửa được lịch đang chạy.

- 07:00–12:00; mặc định 6 giây thực = 1 phút game (30 phút nếu không tiến nhanh).
- Học 08:00–09:00: đủ giờ +12 học tập, vắng hết -6, nội suy theo số phút.
- Làm 09:30–10:30: 500đ/phút tham gia, vắng hết -4 quan hệ, nội suy theo số phút.
- Ăn 15 phút, trả 20.000đ lúc bắt đầu; hoàn thành +20 năng lượng/+3 sức khỏe.
  Hủy không hoàn tiền và không có phần thưởng hồi phục; được báo trước.
- Nghỉ 20 phút, hoàn thành +15 năng lượng/+1 sức khỏe.
- Năng lượng giảm 0,04/phút, khi làm 0,12/phút; chỉ số ngoài tiền giới hạn 0–100.

Lát cắt này không cố đạt thời lượng 25–35 phút khi dùng tiến nhanh; cần cân bằng sau M3.
Chưa có đổi ngày/tuần, ngắt bởi cốt truyện, cam kết, lưu/tải, va chạm trừ chỉ số hay UI chính thức.
Chưa ghép việc học/làm với cơ chế tình huống; hậu quả hiện chỉ là hậu quả của lịch.

## Kiểm tra tự động

Build ngày 04/11/2026 thành công: 0 lỗi, 0 cảnh báo build, Unity thoát mã 0.
Log: `Logs/M2/build-20261104-154458.log`. Scene đã có tham chiếu Config, Player,
camera, xe và bốn ActivityStation; GUID script khớp metadata trong project.
Chưa trực tiếp chạy thao tác chơi hoặc kiểm tra hình ảnh M2.

MorningChecks chạy trong Unity batch trước khi build M2; luật nằm trong Core không phụ thuộc Unity.
Kiểm tra pause; lịch không bị sửa qua cấu hình ngoài; từ chối học ngoài giờ; trả tiền ăn một lần;
hoàn tất ăn dừng bước tiến nhanh; dừng tại mốc bắt đầu lớp; chuyên cần một phần;
ngắt/nối ca làm không cộng phút nghỉ; chốt lương một lần; giới hạn cuối buổi;
ăn đi qua mốc lớp không mất tiến độ; hủy ăn không nhận hồi phục.

## Chờ chơi thử

### Phản hồi tương tác học/làm

Người dùng báo ăn sáng được nhưng học/làm không thấy phản hồi. Chưa biết giờ game
hoặc bảng có nhận raycast trong lần đó. Kiểm tra code xác nhận học/làm bị khóa ngoài
khung lịch, còn thông báo lỗi nằm thấp trong vùng cuộn.
Đã dùng CanStart chung cho UI/luật, hiển thị giờ mở/đóng và số phút còn chờ ở đầu bảng,
đưa thông báo ra ngoài vùng cuộn và cuộn về đầu khi mở tương tác mới.
Thêm dấu ngắm + và phản hồi F khi chưa nhắm đúng, còn trên xe hoặc đang hoạt động.
Không thay giờ lịch, không cho học/làm ngoài giờ và không tự cộng thưởng khi nhấn F.
Kiểm tra luật bổ sung thông báo đến sớm/đã hết giờ; hình ảnh và thao tác cần thử lại.

- [ ] Bảng chỉ dẫn nhìn thấy, F đúng điểm, không tương tác xuyên tường/từ xa/trên xe.
- [ ] Hướng dẫn không trôi giờ; Esc và mất focus dừng giờ.
- [ ] Tab vẫn trôi giờ, con trỏ và khóa điều khiển đúng khi đóng/mở bảng.
- [ ] Ăn, nghỉ, học, làm và dừng giữa chừng phản hồi đúng trong scene thực tế.
- [ ] Camera/xe M1 không bị ảnh hưởng; tổng kết hiển thị rõ.
- [ ] Bản EXE có hình và chạy hết buổi sáng.

Không coi build thành công hoặc kiểm tra Core là nghiệm thu các mục tương tác này.
