--Заполнить свойство BusinUnitDirRX в справочнике Employee значением из справочника Department.BusinesUnit.
if exists (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'Sungero_Core_Recipient' AND COLUMN_NAME = 'BusinUnitDirRX_EssPlS_DirRX')
  begin 
    UPDATE r1
    SET r1.BusinUnitDirRX_EssPlS_DirRX = r2.BusinessUnit_Company_Sungero FROM Sungero_Core_Recipient r1
    JOIN Sungero_Core_Recipient r2
    ON r1.Department_Company_Sungero = r2.Id
  end