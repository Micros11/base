DO $$
begin
--Создание временной таблицы SungeroWfTaskPdfConvertError, в которую записываются данные из задачи AcquaintanceTask, SignTask. А именно свойства колонки ConvertError1, ConvertError.
  if exists (SELECT * FROM information_schema.tables WHERE table_name = 'sungerowftaskpdfconverterror') 
  then
    drop table SungeroWfTaskPdfConvertError;
  end if; 


--Данные копируется если тип свойства Строка nvarchar(250) или другое число в скобках. Тип свойства Текст имеет параметры nvarchar(max).
  if exists (SELECT * FROM information_schema.columns WHERE table_name = 'sungero_wf_task' and column_name = 'converterror1_hrlite_dirrx')
    and exists (SELECT * FROM information_schema.columns WHERE table_name = 'sungero_wf_task' AND column_name = 'converterror_hrlite_dirrx')
    and exists (SELECT * FROM information_schema.columns WHERE table_name = 'sungero_wf_task' AND column_name = 'converterror_hrmanag_dirrx')
  then
    create table sungerowftaskpdfconverterror
    (
      Id int,
      ConvertError1_HRLite_DirRX text,
      ConvertError_HRLite_DirRX text,
      ConvertError_HRManag_DirRX text
    );
 
    insert into sungerowftaskpdfconverterror (Id, ConvertError1_HRLite_DirRX, ConvertError_HRLite_DirRX, ConvertError_HRManag_DirRX)
    select
      s.id,
      s.ConvertError1_HRLite_DirRX,
      s.ConvertError_HRLite_DirRX,
      s.ConvertError_HRManag_DirRX
    from Sungero_WF_Task as s;
  
  end if; 
end$$;