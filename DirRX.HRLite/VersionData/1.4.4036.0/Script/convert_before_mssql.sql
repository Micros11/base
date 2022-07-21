--Создание временной таблицы SungeroWfTaskPdfConvertError, в которую записываются данные из задачи AcquaintanceTask, SignTask. А именно свойства колонки ConvertError1, ConvertError.
if exists (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SungeroWfTaskPdfConvertError')
drop table SungeroWfTaskPdfConvertError

--Данные копируется если тип свойства Строка nvarchar(250) или другое число в скобках. Тип свойства Текст имеет параметры nvarchar(max).
if exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Sungero_WF_Task' AND COLUMN_NAME = 'ConvertError1_HRLite_DirRX' AND DATA_TYPE = 'nvarchar' AND CHARACTER_MAXIMUM_LENGTH <> -1)
and exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Sungero_WF_Task' AND COLUMN_NAME = 'ConvertError_HRLite_DirRX' AND DATA_TYPE = 'nvarchar' AND CHARACTER_MAXIMUM_LENGTH <> -1)
and exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Sungero_WF_Task' AND COLUMN_NAME = 'ConvertError_HRManag_DirRX' AND DATA_TYPE = 'nvarchar' AND CHARACTER_MAXIMUM_LENGTH <> -1)
begin
	execute ('CREATE TABLE SungeroWfTaskPdfConvertError (Id integer, ConvertError1_HRLite_DirRX nvarchar(max), ConvertError_HRLite_DirRX nvarchar(max), ConvertError_HRManag_DirRX nvarchar(max))
				INSERT INTO SungeroWfTaskPdfConvertError
				SELECT s.Id, s.ConvertError1_HRLite_DirRX, s.ConvertError_HRLite_DirRX, s.ConvertError_HRManag_DirRX FROM Sungero_WF_Task s')
end