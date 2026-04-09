IF DB_ID(N'TrafficSafetyDB') IS NULL
BEGIN
    CREATE DATABASE [TrafficSafetyDB];
END
GO

USE [TrafficSafetyDB];
GO

SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.OFFICERS', N'U') IS NOT NULL DROP TABLE dbo.OFFICERS;
IF OBJECT_ID(N'dbo.ADMINS', N'U') IS NOT NULL DROP TABLE dbo.ADMINS;
IF OBJECT_ID(N'dbo.USERS', N'U') IS NOT NULL DROP TABLE dbo.USERS;
GO

CREATE TABLE dbo.USERS
(
    CCCD VARCHAR(12) NOT NULL PRIMARY KEY,
    FULL_NAME NVARCHAR(100) NOT NULL,
    DOB DATE NULL,
    EMAIL VARCHAR(100) NOT NULL,
    PHONE VARCHAR(15) NOT NULL,
    PASS_HASH NVARCHAR(100) NOT NULL,
    GENDER NVARCHAR(10) NULL,
    IMAGE_PATH NVARCHAR(255) NULL
);
GO

CREATE TABLE dbo.OFFICERS
(
    BADGE_NUMBER VARCHAR(20) NOT NULL PRIMARY KEY,
    FULL_NAME NVARCHAR(100) NOT NULL,
    PASS_HASH NVARCHAR(100) NOT NULL,
    IMAGE_PATH NVARCHAR(255) NULL
);
GO

CREATE TABLE dbo.ADMINS
(
    ADMIN_ID VARCHAR(20) NOT NULL PRIMARY KEY,
    USERNAME VARCHAR(50) NOT NULL UNIQUE,
    FULL_NAME NVARCHAR(100) NOT NULL,
    PASS_HASH NVARCHAR(100) NOT NULL,
    IMAGE_PATH NVARCHAR(255) NULL
);
GO

INSERT INTO dbo.USERS (CCCD, FULL_NAME, DOB, EMAIL, PHONE, PASS_HASH, GENDER, IMAGE_PATH)
VALUES
('068901234567', N'Bùi Gia Bảo', CONVERT(date, '07/12/1997', 103), 'giabao68@gmail.com', '0321112233', N'giabao68', N'Nam', NULL),
('028901234567', N'Bùi Gia Linh', CONVERT(date, '07/06/2000', 103), 'gialinh28@gmail.com', '0389017788', N'gialinh28', N'Nam', NULL),
('048901234567', N'Bùi Khánh Vy', CONVERT(date, '05/07/2005', 103), 'khanhvy48@gmail.com', '0789900112', N'khanhvy48', N'Nữ', NULL),
('088901234567', N'Bùi Minh Thảo', CONVERT(date, '11/10/2003', 103), 'minhthao88@gmail.com', '0746677889', N'minhthao88', N'Nam', NULL),
('008901234567', N'Bùi Thanh Tùng', CONVERT(date, '27/05/2003', 103), 'thanhtung08@gmail.com', '0391234567', N'thanhtung08', N'Nam', NULL),
('076789012345', N'Cao Đức Anh', CONVERT(date, '25/10/1998', 103), 'ducanh76@gmail.com', '0521113344', N'ducanh76', N'Nam', NULL),
('096789012345', N'Cao Gia Hân', CONVERT(date, '15/03/2001', 103), 'giahan96@gmail.com', '0835455667', N'giahan96', N'Nữ', NULL),
('056789012345', N'Cao Ngọc Anh', CONVERT(date, '22/11/1998', 103), 'ngocanh56@gmail.com', '0878788990', N'ngocanh56', N'Nữ', NULL),
('016789012345', N'Cao Quốc Khánh', CONVERT(date, '10/09/1998', 103), 'quockhanh16@gmail.com', '0945455667', N'quockhanh16', N'Nam', NULL),
('036789012345', N'Cao Thanh Trúc', CONVERT(date, '01/01/2006', 103), 'thanhtruc36@gmail.com', '0566789012', N'thanhtruc36', N'Nữ', NULL),
('092345678901', N'Dương Bảo Vy', CONVERT(date, '12/01/2006', 103), 'baovy92@gmail.com', '0781011223', N'baovy92', N'Nữ', NULL),
('012345678901', N'Dương Gia Bảo', CONVERT(date, '09/08/2001', 103), 'giabao12@gmail.com', '0891011223', N'giabao12', N'Nam', NULL),
('052345678901', N'Dương Gia Linh', CONVERT(date, '13/10/1999', 103), 'gialinh52@gmail.com', '0834344556', N'gialinh52', N'Nữ', NULL),
('072345678901', N'Dương Ngọc Hân', CONVERT(date, '25/03/2004', 103), 'ngochan72@gmail.com', '0365556677', N'ngochan72', N'Nữ', NULL),
('032345678901', N'Dương Thị Lan', CONVERT(date, '10/08/1998', 103), 'thilan32@gmail.com', '0543452122', N'thilan32', N'Nữ', NULL),
('009012345678', N'Đặng Khánh Linh', CONVERT(date, '14/11/1999', 103), 'khanhlinh09@gmail.com', '0522345678', N'khanhlinh09', N'Nữ', NULL),
('049012345678', N'Đặng Minh Hoàng', CONVERT(date, '13/08/1997', 103), 'minhhoang49@gmail.com', '0791011223', N'minhhoang49', N'Nam', NULL),
('029012345678', N'Đặng Thanh Bình', CONVERT(date, '25/02/2003', 103), 'thanhbinh29@gmail.com', '0390128899', N'thanhbinh29', N'Nam', NULL),
('089012345678', N'Đặng Thanh Nhàn', CONVERT(date, '07/08/2006', 103), 'thanhnhan89@gmail.com', '0757788990', N'thanhnhan89', N'Nữ', NULL),
('069012345678', N'Đặng Thảo Nguyên', CONVERT(date, '28/12/2002', 103), 'thaonguyen69@gmail.com', '0332223344', N'thaonguyen69', N'Nữ', NULL),
('038901234567', N'Đinh Kim Ngân', CONVERT(date, '04/08/1995', 103), 'kimngan38@gmail.com', '0588901234', N'kimngan38', N'Nam', NULL),
('018901234567', N'Đinh Ngọc Hân', CONVERT(date, '26/05/1997', 103), 'ngochan18@gmail.com', '0967677889', N'ngochan18', N'Nữ', NULL),
('078901234567', N'Đinh Ngọc Huyền', CONVERT(date, '12/12/1996', 103), 'ngochuyen78@gmail.com', '0543335566', N'ngochuyen78', N'Nữ', NULL),
('098901234567', N'Đinh Quốc Bảo', CONVERT(date, '19/08/2003', 103), 'quocbao98@gmail.com', '0857677889', N'quocbao98', N'Nam', NULL),
('058901234567', N'Đinh Thu Trang', CONVERT(date, '07/06/2000', 103), 'thutrang58@gmail.com', '0890900112', N'thutrang58', N'Nữ', NULL),
('066789012345', N'Đỗ Bảo Trân', CONVERT(date, '28/08/2005', 103), 'baotran66@gmail.com', '0988788990', N'baotran66', N'Nữ', NULL),
('046789012345', N'Đỗ Hồng Nhung', CONVERT(date, '01/01/2000', 103), 'hongnhung46@gmail.com', '0767788990', N'hongnhung46', N'Nữ', NULL),
('026789012345', N'Đỗ Minh Thư', CONVERT(date, '09/11/2003', 103), 'minhthu26@gmail.com', '0367895566', N'minhthu26', N'Nam', NULL),
('086789012345', N'Đỗ Thanh Phong', CONVERT(date, '22/04/1998', 103), 'thanhphong86@gmail.com', '0724455667', N'thanhphong86', N'Nam', NULL),
('006789012345', N'Đỗ Tuấn Kiệt', CONVERT(date, '04/06/1996', 103), 'tuankiet06@gmail.com', '0379012345', N'tuankiet06', N'Nam', NULL),
('020123456789', N'Hoàng Bảo Ngọc', CONVERT(date, '02/01/1995', 103), 'baongoc20@gmail.com', '0989899001', N'baongoc20', N'Nữ', NULL),
('080123456789', N'Hoàng Gia Linh', CONVERT(date, '08/07/2002', 103), 'gialinh80@gmail.com', '0565557788', N'gialinh80', N'Nữ', NULL),
('100123456789', N'Hoàng Ngọc Anh', CONVERT(date, '25/03/2007', 103), 'ngocanh100@gmail.com', '0879899001', N'ngocanh100', N'Nữ', NULL),
('040123456789', N'Hoàng Phúc', CONVERT(date, '08/04/2007', 103), 'hoangphuc40@gmail.com', '0701122334', N'hoangphuc40', N'Nam', NULL),
('060123456789', N'Hoàng Thanh Tâm', CONVERT(date, '07/11/2004', 103), 'thanhtam60@gmail.com', '0922122334', N'thanhtam60', N'Nữ', NULL),
('090123456789', N'Hồ Minh Anh', CONVERT(date, '26/10/1999', 103), 'minhanh90@gmail.com', '0768899001', N'minhanh90', N'Nữ', NULL),
('070123456789', N'Hồ Minh Nhật', CONVERT(date, '01/02/1997', 103), 'minhnhat70@gmail.com', '0343334455', N'minhnhat70', N'Nam', NULL),
('010123456789', N'Hồ Minh Quân', CONVERT(date, '27/11/2000', 103), 'minhquan10@gmail.com', '0533456789', N'minhquan10', N'Nam', NULL),
('050123456789', N'Hồ Ngọc Diễm', CONVERT(date, '03/01/2000', 103), 'ngocdiem50@gmail.com', '0812122334', N'ngocdiem50', N'Nữ', NULL),
('030123456789', N'Hồ Quỳnh Như', CONVERT(date, '16/02/2004', 103), 'quynhnhu30@gmail.com', '0521239900', N'quynhnhu30', N'Nữ', NULL),
('043456789012', N'Lê Gia Bảo', CONVERT(date, '22/10/2002', 103), 'giabao43@gmail.com', '0734455667', N'giabao43', N'Nam', NULL),
('083456789012', N'Lê Gia Minh', CONVERT(date, '07/12/1996', 103), 'giaminh83@gmail.com', '0598881011', N'giaminh83', N'Nam', NULL),
('063456789012', N'Lê Hoàng Anh', CONVERT(date, '01/08/1999', 103), 'hoanganh63@gmail.com', '0955455667', N'hoanganh63', N'Nữ', NULL),
('003456789012', N'Lê Hoàng Nam', CONVERT(date, '14/12/2007', 103), 'hoangnam03@gmail.com', '0346789012', N'hoangnam03', N'Nam', NULL),
('023456789012', N'Lê Văn Hùng', CONVERT(date, '27/01/2001', 103), 'vanhung23@gmail.com', '0334562233', N'vanhung23', N'Nam', NULL),
('019012345678', N'Lưu Công Thành', CONVERT(date, '26/03/2001', 103), 'congthanh19@gmail.com', '0978788990', N'congthanh19', N'Nam', NULL),
('059012345678', N'Lưu Gia Hân', CONVERT(date, '17/09/2001', 103), 'giahan59@gmail.com', '0911011223', N'giahan59', N'Nam', NULL),
('039012345678', N'Lưu Minh Trí', CONVERT(date, '14/09/2003', 103), 'minhtri39@gmail.com', '0599012345', N'minhtri39', N'Nữ', NULL),
('079012345678', N'Lưu Quốc Trung', CONVERT(date, '24/12/1997', 103), 'quoctrung79@gmail.com', '0554446677', N'quoctrung79', N'Nam', NULL),
('099012345678', N'Lưu Thùy Linh', CONVERT(date, '19/03/1999', 103), 'thuylinh99@gmail.com', '0868788990', N'thuylinh99', N'Nữ', NULL),
('013456789012', N'Lý Hải Dương', CONVERT(date, '21/10/1998', 103), 'haidung13@gmail.com', '0912122334', N'haidung13', N'Nam', NULL),
('033456789012', N'Lý Minh Khang', CONVERT(date, '18/03/2005', 103), 'minhkhang33@gmail.com', '0554563233', N'minhkhang33', N'Nam', NULL),
('073456789012', N'Lý Minh Quang', CONVERT(date, '10/01/2005', 103), 'minhquang73@gmail.com', '0376667788', N'minhquang73', N'Nam', NULL),
('053456789012', N'Lý Quốc Huy', CONVERT(date, '02/11/1997', 103), 'quochuy53@gmail.com', '0845455667', N'quochuy53', N'Nam', NULL),
('093456789012', N'Lý Thanh Tùng', CONVERT(date, '25/10/2001', 103), 'thanhtung93@gmail.com', '0792122334', N'thanhtung93', N'Nam', NULL),
('035678901234', N'Mai Gia Huy', CONVERT(date, '20/08/1997', 103), 'giahuy35@gmail.com', '0555678901', N'giahuy35', N'Nam', NULL),
('075678901234', N'Mai Mai Anh', CONVERT(date, '01/08/1998', 103), 'maianh75@gmail.com', '0398889900', N'maianh75', N'Nữ', NULL),
('055678901234', N'Mai Minh Đức', CONVERT(date, '24/06/2001', 103), 'minhduc55@gmail.com', '0867677889', N'minhduc55', N'Nam', NULL),
('095678901234', N'Mai Quốc Việt', CONVERT(date, '11/10/1996', 103), 'quocviet95@gmail.com', '0824344556', N'quocviet95', N'Nam', NULL),
('015678901234', N'Mai Quỳnh Anh', CONVERT(date, '08/01/2001', 103), 'quynhanh15@gmail.com', '0934344556', N'quynhanh15', N'Nữ', NULL),
('067890123456', N'Ngô Đức Thịnh', CONVERT(date, '18/03/2002', 103), 'ducthinh67@gmail.com', '0999899001', N'ducthinh67', N'Nam', NULL),
('047890123456', N'Ngô Gia Minh', CONVERT(date, '01/10/1996', 103), 'giaminh47@gmail.com', '0778899001', N'giaminh47', N'Nam', NULL),
('007890123456', N'Ngô Ngọc Anh', CONVERT(date, '16/07/2007', 103), 'ngocanh07@gmail.com', '0380123456', N'ngocanh07', N'Nữ', NULL),
('087890123456', N'Ngô Quốc Hưng', CONVERT(date, '28/11/1999', 103), 'quochung87@gmail.com', '0735566778', N'quochung87', N'Nam', NULL),
('027890123456', N'Ngô Quốc Việt', CONVERT(date, '02/08/2006', 103), 'quocviet27@gmail.com', '0378906677', N'quocviet27', N'Nam', NULL),
('081234567890', N'Nguyễn Bảo An', CONVERT(date, '13/05/2005', 103), 'baoan81@gmail.com', '0576668899', N'baoan81', N'Nữ', NULL),
('041234567890', N'Nguyễn Bích Ngọc', CONVERT(date, '27/03/2000', 103), 'bichngoc41@gmail.com', '0712233445', N'bichngoc41', N'Nữ', NULL),
('001234567890', N'Nguyễn Minh Anh', CONVERT(date, '25/06/2001', 103), 'minhanh01@gmail.com', '0324567890', N'minhanh01', N'Nữ', NULL),
('061234567890', N'Nguyễn Minh Châu', CONVERT(date, '02/02/2002', 103), 'minhchau61@gmail.com', '0933233445', N'minhchau61', N'Nam', NULL),
('021234567890', N'Nguyễn Trung Hiếu', CONVERT(date, '02/10/2001', 103), 'trunghieu21@gmail.com', '0990900112', N'trunghieu21', N'Nam', NULL),
('004567890123', N'Phạm Gia Hân', CONVERT(date, '28/06/2006', 103), 'giahan04@gmail.com', '0357890123', N'giahan04', N'Nữ', NULL),
('064567890123', N'Phạm Gia Huy', CONVERT(date, '13/07/1996', 103), 'giahuy64@gmail.com', '0966566778', N'giahuy64', N'Nam', NULL),
('084567890123', N'Phạm Ngọc Huy', CONVERT(date, '27/10/2002', 103), 'ngochuy84@gmail.com', '0702233445', N'ngochuy84', N'Nam', NULL),
('024567890123', N'Phạm Ngọc Mai', CONVERT(date, '27/11/2003', 103), 'ngocmai24@gmail.com', '0345673344', N'ngocmai24', N'Nữ', NULL),
('044567890123', N'Phạm Thanh Hà', CONVERT(date, '20/01/1995', 103), 'thanhha44@gmail.com', '0745566778', N'thanhha44', N'Nữ', NULL),
('031234567890', N'Phan Đức Huy', CONVERT(date, '06/12/1995', 103), 'duchuy31@gmail.com', '0532341011', N'duchuy31', N'Nam', NULL),
('091234567890', N'Phan Gia Huy', CONVERT(date, '06/09/1998', 103), 'giahuy91@gmail.com', '0779900112', N'giahuy91', N'Nam', NULL),
('071234567890', N'Phan Hải Dương', CONVERT(date, '08/12/2003', 103), 'haidung71@gmail.com', '0354445566', N'haidung71', N'Nam', NULL),
('011234567890', N'Phan Thảo Vy', CONVERT(date, '01/06/2005', 103), 'thaovy11@gmail.com', '0880900112', N'thaovy11', N'Nữ', NULL),
('051234567890', N'Phan Tuấn Anh', CONVERT(date, '24/10/1999', 103), 'tuananh51@gmail.com', '0823233445', N'tuananh51', N'Nam', NULL),
('082345678901', N'Trần Hoàng Yến', CONVERT(date, '01/08/1995', 103), 'hoangyen82@gmail.com', '0587779900', N'hoangyen82', N'Nữ', NULL),
('042345678901', N'Trần Minh Tâm', CONVERT(date, '02/11/2002', 103), 'minhtam42@gmail.com', '0723344556', N'minhtam42', N'Nữ', NULL),
('002345678901', N'Trần Quốc Bảo', CONVERT(date, '13/06/2006', 103), 'quocbao02@gmail.com', '0335678901', N'quocbao02', N'Nam', NULL),
('062345678901', N'Trần Quốc Bảo', CONVERT(date, '21/07/2006', 103), 'quocbao62@gmail.com', '0944344556', N'quocbao62', N'Nam', NULL),
('022345678901', N'Trần Thu Hà', CONVERT(date, '22/05/2000', 103), 'thuha22@gmail.com', '0323451122', N'thuha22', N'Nữ', NULL),
('074567890123', N'Trịnh Gia Khánh', CONVERT(date, '16/07/1996', 103), 'giakhanh74@gmail.com', '0387778899', N'giakhanh74', N'Nam', NULL),
('094567890123', N'Trịnh Minh Thư', CONVERT(date, '12/02/1995', 103), 'minhthu94@gmail.com', '0813233445', N'minhthu94', N'Nữ', NULL),
('034567890123', N'Trịnh Mỹ Linh', CONVERT(date, '18/07/1998', 103), 'mylinh34@gmail.com', '0544567890', N'mylinh34', N'Nữ', NULL),
('014567890123', N'Trịnh Nhật Minh', CONVERT(date, '04/06/2002', 103), 'nhatminh14@gmail.com', '0923233445', N'nhatminh14', N'Nam', NULL),
('054567890123', N'Trịnh Thùy Dương', CONVERT(date, '22/12/2002', 103), 'thuyduong54@gmail.com', '0856566778', N'thuyduong54', N'Nam', NULL),
('025678901234', N'Võ Anh Tuấn', CONVERT(date, '01/04/2003', 103), 'anhtuan25@gmail.com', '0356784455', N'anhtuan25', N'Nam', NULL),
('005678901234', N'Võ Đức Anh', CONVERT(date, '05/06/1997', 103), 'ducanh05@gmail.com', '0368901234', N'ducanh05', N'Nam', NULL),
('085678901234', N'Võ Khánh Linh', CONVERT(date, '03/09/2007', 103), 'khanhlinh85@gmail.com', '0713344556', N'khanhlinh85', N'Nữ', NULL),
('065678901234', N'Võ Minh Anh', CONVERT(date, '02/09/2007', 103), 'minhanh65@gmail.com', '0977677889', N'minhanh65', N'Nam', NULL),
('045678901234', N'Võ Quốc Đạt', CONVERT(date, '04/07/2003', 103), 'quocdat45@gmail.com', '0756677889', N'quocdat45', N'Nam', NULL),
('057890123456', N'Vũ Hải Nam', CONVERT(date, '11/03/2001', 103), 'hainam57@gmail.com', '0889899001', N'hainam57', N'Nam', NULL),
('017890123456', N'Vũ Hoài Nam', CONVERT(date, '05/11/1997', 103), 'hoainam17@gmail.com', '0956566778', N'hoainam17', N'Nam', NULL),
('077890123456', N'Vũ Minh Khoa', CONVERT(date, '09/04/2001', 103), 'minhkhoa77@gmail.com', '0532224455', N'minhkhoa77', N'Nam', NULL),
('097890123456', N'Vũ Minh Tuấn', CONVERT(date, '17/12/1998', 103), 'minhtuan97@gmail.com', '0846566778', N'minhtuan97', N'Nam', NULL),
('037890123456', N'Vũ Quốc An', CONVERT(date, '26/05/2003', 103), 'quocan37@gmail.com', '0577890123', N'quocan37', N'Nam', NULL);
GO

INSERT INTO dbo.OFFICERS (BADGE_NUMBER, FULL_NAME, PASS_HASH, IMAGE_PATH)
VALUES
('CA1234', N'Bùi Gia Bảo', N'giabao68', NULL),
('CA1245', N'Bùi Gia Linh', N'gialinh28', NULL),
('CA1254', N'Bùi Khánh Vy', N'khanhvy48', NULL),
('CA7624', N'Bùi Minh Thảo', N'minhthao88', NULL),
('CA6381', N'Bùi Thanh Tùng', N'thanhtung08', NULL),
('CA4231', N'Cao Đức Anh', N'ducanh76', NULL),
('CA8736', N'Cao Gia Hân', N'giahan96', NULL),
('CA7364', N'Cao Ngọc Anh', N'ngocanh56', NULL),
('CA8822', N'Cao Quốc Khánh', N'quockhanh16', NULL),
('CA7612', N'Cao Thanh Trúc', N'thanhtruc36', NULL),
('CA5342', N'Dương Bảo Vy', N'baovy92', NULL),
('CA9891', N'Dương Gia Bảo', N'giabao12', NULL),
('CA6641', N'Dương Gia Linh', N'gialinh52', NULL),
('CA1111', N'Dương Ngọc Hân', N'ngochan72', NULL),
('CA2312', N'Dương Thị Lan', N'thilan32', NULL),
('CA9241', N'Đặng Khánh Linh', N'khanhlinh09', NULL),
('CA8124', N'Đặng Minh Hoàng', N'minhhoang49', NULL),
('CA7761', N'Đặng Thanh Bình', N'thanhbinh29', NULL),
('CA6512', N'Đặng Thanh Nhàn', N'thanhnhan89', NULL),
('CA8881', N'Đặng Thảo Nguyên', N'thaonguyen69', NULL);
GO

INSERT INTO dbo.ADMINS (ADMIN_ID, USERNAME, FULL_NAME, PASS_HASH, IMAGE_PATH)
VALUES
('AD01', 'admin1', N'Quản trị viên 1', N'admin', NULL),
('AD02', 'admin2', N'Quản trị viên 2', N'admin', NULL),
('AD03', 'admin3', N'Quản trị viên 3', N'admin', NULL),
('AD04', 'admin4', N'Quản trị viên 4', N'admin', NULL);
GO

SELECT COUNT(*) AS UserCount FROM dbo.USERS;
SELECT COUNT(*) AS OfficerCount FROM dbo.OFFICERS;
SELECT COUNT(*) AS AdminCount FROM dbo.ADMINS;
GO
