DO $$
begin
--Заполнить свойство BusinUnitDirRX в справочнике Employee значением из справочника Department.BusinesUnit.
	if exists (SELECT * FROM information_schema.columns
              WHERE table_name = 'sungero_core_recipient' and column_name = 'businunitdirrx_esspls_dirrx')
  then
	UPDATE Sungero_Core_Recipient
    SET BusinUnitDirRX_EssPlS_DirRX = r2.BusinessUnit_Company_Sungero
    FROM Sungero_Core_Recipient r2
    WHERE Sungero_Core_Recipient.Department_Company_Sungero = r2.Id;
	end if;
end$$;