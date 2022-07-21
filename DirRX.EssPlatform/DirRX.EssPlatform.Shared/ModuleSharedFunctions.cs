using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.EssPlatform.Shared
{
  public class ModuleFunctions
  {
    /// <summary>
    /// Получить значимые символы из номера телефона(знак + в начале номера и последующие цифры), все остальные символы игнорируются.
    /// </summary>
    /// <param name="phone">Номер телефона.</param>
    /// <returns>Номер телефона, только цифры и знак + в начале, если он присутствует.</returns>
    [Public]
    public string GetSignificantSymbolsFromPhoneNumber(string phone)
    {
      if (string.IsNullOrEmpty(phone))
        return string.Empty;

      var result = new string(phone.ToCharArray().Where(n => n >= '0' && n <= '9' || n == '+').ToArray());

      if (!string.IsNullOrEmpty(result) && result[0] == '+')
        return string.Format("+{0}", result.Replace("+", string.Empty));
      else
        return result.Replace("+", string.Empty);
    }
    
    /// <summary>
    /// Определить, корректен ли номер личного номера телефона.
    /// Если присутствует символ +, то в номере должно быть 11 цифр,
    /// если в номере нет +, то в номере должно быть строго 11 или 11 или 10 цифр (зависит от параметра needElevenDigits).
    /// </summary>
    /// <param name="phone">Номер телефона.</param>
    /// <param name="needElevenDigits">Требовать обязательно 11 цифр номера.</param>
    /// <returns>Да - если правильная длина, иначе - ложь.</returns>
    [Public]
    public bool PhoneNumberLengthIsValid(string phone, bool needElevenDigits)
    {
      var phoneSymbols = this.GetSignificantSymbolsFromPhoneNumber(phone);

      if (!string.IsNullOrEmpty(phoneSymbols) && 
          ((phoneSymbols.Length == 12 && phoneSymbols[0] == '+') || 
           (phoneSymbols[0] != '+' && (phoneSymbols.Length == 11 || (!needElevenDigits && phoneSymbols.Length == 10)))))
        return true;

      return false;
    }
    
    /// <summary>
    /// Отформатировать номер телефона в универсальный формат. Предварительно нужно проверить валидность номера (PhoneNumberLengthIsValid).
    /// </summary>
    /// <param name="phone">Номер телефона.</param>
    /// <returns>Номер телефона в формате +7 (999)-999-99-99.</returns>
    [Public]
    public string PhoneNumberToUniversalFormat(string phone)
    {
      if (string.IsNullOrEmpty(phone))
        return string.Empty;

      var number = this.GetSignificantSymbolsFromPhoneNumber(phone);

      if (number.Length < 10)
      {
        Logger.Error(Resources.ErrorPhoneNumberDigitsFormat(phone));
        throw new ArgumentException(Resources.ErrorPhoneNumberDigitsFormat(phone));
      }

      if (number.Length == 10)
        number = "+7" + number;
      if (number.Length == 11 && number[0] == '8')
        number = "+7" + number.Substring(1, 10);
      else
        if (number.Length == 11)
          number = "+" + number;
      
      return string.Format("{0} ({1}) {2}-{3}-{4}", number.Substring(0, 2), number.Substring(2, 3), number.Substring(5, 3), number.Substring(8, 2), number.Substring(10, 2));
    }
  }
}