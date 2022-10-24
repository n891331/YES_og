//function(多檔上傳區塊的Id,選擇檔案清單區塊的Id,選擇檔案的陣列)
$.extend({
    MultiUploadFile: function (upfileAreaId, fileListAreaId, filesArr) {
        var $upFileArea = $("#" + upfileAreaId);
        var $inputFile = $upFileArea.find("input");
        var $fileListArea = $("#" + fileListAreaId);

        $inputFile.on('change', function (e) {
            //addFiles
            var tmp;
            if (e.dataTransfer) {
                tmp = e.dataTransfer.files;
            } else if (e.target) {
                tmp = e.target.files;
            }

            // 檢查 -- Start
            var interrupt = false;
            var maxSize = $(this).attr('max-size');
            var acceptExtension = $(this).attr('accept-extension');

            for (var i = 0; i < tmp.length; i++) {
                var fileName = tmp.item(i).name;

                // 檢查檔名是否有特殊字元 -- Start
                //var reSpecialChar = new RegExp("[`~!@#$^&*()=|{}':;',\\[\\].<>/?~！@#￥……&*（）&mdash;—|{}【】‘；：”“'。，、？]")
                var reSpecialChar = new RegExp("[#%+\/:*?\"<>|]");
                var fileNameNoExt = fileName.substring(0, fileName.indexOf(fileName.split(".")[fileName.split(".").length - 1]) - 1);

                if (reSpecialChar.test(fileNameNoExt)) {
                    alert("檔名不得含有特殊字元：\n#%+\/:*?\"<>|");
                    interrupt = true;
                    break;
                }
                // 檢查檔名是否有特殊字元 -- End

                // 檢查是否檔案過大 -- Start
                var fileSize = tmp.item(i).size;

                if (!isNaN(parseInt(maxSize, 10))) {
                    if (fileSize > parseInt(maxSize, 10)) {
                        alert("檔案大小超過" + parseInt(maxSize) + " Bytes：" + fileName);
                        interrupt = true;
                        break;
                    }
                }
                // 檢查是否檔案過 -- End

                // 檢查是否有重複檔名 -- Start
                for (var j = 0; j < filesArr.length; j++) {
                    if (fileName === filesArr[j]["name"]) {
                        interrupt = true;
                        alert("已選擇檔案：" + fileName);
                        break;
                    }
                }
                // 檢查是否有重複檔名 -- End

                // 檢查檔案類型 -- Start
                var reExt = new RegExp(".(" + acceptExtension + ")$", "i");  //允許的圖片副檔名 
                if (!reExt.test(fileName)) {
                    alert("只允許上傳以下類型的檔案：\n" + acceptExtension);
                    interrupt = true;
                    break;
                }
                // 檢查檔案類型 -- End

                if (interrupt) {
                    break;
                }
            }

            if (interrupt) {
                $inputFile.wrap('<form>').closest('form').get(0).reset();
                $inputFile.unwrap();
                return;
            }
            // 檢查 -- End

            //將新選擇的檔案塞入檔案陣列
            for (var i = 0; i < tmp.length; i++) {
                filesArr.push(tmp.item(i));
            }

            $inputFile.wrap('<form>').closest('form').get(0).reset();
            $inputFile.unwrap();

            //showFiles
            $fileListArea.html("");
            fileNum = filesArr.length;
            for (var i = 0; i < fileNum; i++) {
                $fileListArea.append('<div><span class="fa-stack fa-lg"><i class="fa fa-file fa-stack-1x "></i><strong class="fa-stack-1x" style="color:#FFF; font-size:12px; margin-top:2px;">' + (i + 1) + '</strong></span> ' + filesArr[i].name + '&nbsp;&nbsp;<span class="fa fa-times-circle fa-lg closeBtn" title="remove"></span></div>');
            }
        });

        $fileListArea.on('click', '.closeBtn', function (e) {
            e.preventDefault();
            e.stopPropagation();

            var divElem = $(this).parent();
            var index = $fileListArea.find('div').index(divElem);
            if (index !== -1) {
                $fileListArea[0].removeChild(divElem[0]);
                filesArr.splice(index, 1);
            }
        });

    }
});