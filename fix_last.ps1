
$replacements = @(
    @{"N"="C\uFFFDc "; "V"="Các "},
    @{"N"="n\uFFFDy"; "V"="này"},
    @{"N"="s\u1ED1 "; "V"="sẽ "},
    @{"N"="gi\uFFFDp"; "V"="giúp"},
    @{"N"="giao di\u1EA8n"; "V"="giao diện"},
    @{"N"="T\? \u0111\u1ED3ng"; "V"="Tự động"},
    @{"N"="m\?c \uFFFD\u1EA8nh"; "V"="mặc định"},
    @{"N"="PH\uFFFDN QUY\u1EA8n HI\u1EA8n TH\? MENU"; "V"="PHÂN QUYỀN HIỂN THỊ MENU"},
    @{"N"="C\uFFFDng d\uFFFDn:"; "V"="Công dân:"},
    @{"N"="c\uFFFDc n\uFFFDt chuy\u1EA8n"; "V"="các nút chuyển"},
    @{"N"="k\? ph\?"; "V"="kẻ phụ"},
    @{"N"="C\uFFFDn b\?: \uFFFD\uFFFD\?c"; "V"="Cán bộ: Được"},
    @{"N"="Kh\uFFFDch h\uFFFDng"; "V"="Khách hàng"},
    @{"N"="Qu\u1EA8n Tr\u1EEB vi\uFFFDn"; "V"="Quản trị viên"},
    @{"N"="Hi\u1EA8n T\u1EA5t c\u1EA3 c\uFFFDc"; "V"="Hiện Tất cả các"},
    @{"N"="l\?a ch\u1EA8n \uFFFD\?"; "V"="lựa chọn để"},
    @{"N"="Ki\u1EC3m tra n\?u c\u00D4 t\u00F4n th\? g\uFFFDn v\u00E0o"; "V"="Kiểm tra nếu có tên thì gán vào"},
    @{"N"="L\?p bi\uFFFDn b\u1EA8n"; "V"="Lập biên bản"},
    @{"N"="Ph\u1EA8n \uFFFDnh"; "V"="Phản ánh"},
    @{"N"="\uFFFD\uFFFDng xu\?t"; "V"="Đăng xuất"},
    @{"N"="X\u1EED l\u00FD s\u1ED1 ki\u1EA8n n\uFFFDt"; "V"="Xử lý sự kiện nút"},
    @{"N"="th\u0111\u1ED3ng"; "V"="thường"},
    @{"N"="nh\u1EA8n th\uFFFDng"; "V"="nhận thông"},
    @{"N"="\uFFFD\? x\? l\?"; "V"="Đã xử lý"},
    @{"N"="Ch\u01B0a x\? l\?"; "V"="Chưa xử lý"},
    @{"N"="\uFFFD\? X\u1EED l\u00FD"; "V"="Đã xử lý"},
    @{"N"="\uFFFD\? X\uFFFDc nh\u1EADn"; "V"="Đã xác nhận"},
    @{"N"="\uFFFD\uFFFDnh L\u1ED7i"; "V"="Đánh lại"},
    @{"N"="Quay L\u1ED7i"; "V"="Quay lại"},
    @{"N"="th\uFFFDng tin"; "V"="thông tin"},
    @{"N"="Th\u00F4ng b\u00E1o"; "V"="Thông báo"},
    @{"N"="\uFFFDu ti\uFFFDn"; "V"="Ưu tiên"}
)

$files = Get-ChildItem PBL3\Page*.xaml.cs
foreach ($file in $files) {
    if ($file.Length -gt 0) {
        $filePath = $file.FullName
        $content = [System.IO.File]::ReadAllText($filePath, [System.Text.Encoding]::UTF8)
        $changed = $false
        foreach ($item in $replacements) {
            $pattern = $item["N"]
            if ($content -match $pattern) {
                $content = $content -replace $pattern, $item["V"]
                $changed = $true
            }
        }
        if ($changed) {
            [System.IO.File]::WriteAllText($filePath, $content, [System.Text.Encoding]::UTF8)
        }
    }
}
