if exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SungeroWfTaskPdfConvertError')
and exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Sungero_WF_Task' AND COLUMN_NAME = 'ConvertError1_HRLite_DirRX' )
and exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Sungero_WF_Task' AND COLUMN_NAME = 'ConvertError_HRLite_DirRX')
and exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Sungero_WF_Task' AND COLUMN_NAME = 'ConvertError_HRManag_DirRX')

begin
	UPDATE s
	SET 
		ConvertError1_HRLite_DirRX = tmp.ConvertError1_HRLite_DirRX,
		ConvertError_HRLite_DirRX = tmp.ConvertError_HRLite_DirRX,
		ConvertError_HRManag_DirRX = tmp.ConvertError_HRManag_DirRX
	FROM SungeroWfTaskPdfConvertError tmp
	JOIN Sungero_WF_Task s on tmp.Id = s.Id

	drop table SungeroWfTaskPdfConvertError
end