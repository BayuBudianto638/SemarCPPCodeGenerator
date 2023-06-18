using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemarCPPCodeGenerator.Services
{
    public class CppService
    {
        public string GenerateCppCode(string entity, string[] fields)
        {
            StringBuilder codeBuilder = new StringBuilder();

            codeBuilder.AppendLine($"class {entity}");
            codeBuilder.AppendLine("{");
            codeBuilder.AppendLine("private:");

            foreach (var field in fields)
            {
                codeBuilder.AppendLine($"    {GetFieldType(field)} {field};");
            }

            codeBuilder.AppendLine();
            codeBuilder.AppendLine("public:");
            codeBuilder.AppendLine($"    {entity}()");
            codeBuilder.AppendLine("        : BaseEntity()");

            foreach (var field in fields)
            {
                codeBuilder.AppendLine($"        , {field}({GetFieldInitialization(field)})");
            }

            codeBuilder.AppendLine("    {");
            codeBuilder.AppendLine("    }");
            codeBuilder.AppendLine("}");

            return codeBuilder.ToString();
        }

        private string GetFieldType(string field)
        {
            string[] fieldComponents = field.Split(':');
            if (fieldComponents.Length == 2)
            {
                string fieldType = fieldComponents[1].Trim().ToLower();
                switch (fieldType)
                {
                    case "int":
                        return "int";
                    case "string":
                        return "std::string";
                    case "datetime":
                        return "std::tm";
                    default:
                        return "TBD";
                }
            }
            return "TBD";
        }

        private string GetFieldInitialization(string field)
        {
            string[] fieldComponents = field.Split(':');
            if (fieldComponents.Length == 2)
            {
                string fieldType = fieldComponents[1].Trim().ToLower();
                switch (fieldType)
                {
                    case "int":
                        return "0";
                    case "string":
                        return "\"\"";
                    case "datetime":
                        return "std::tm{}";
                    default:
                        return "TBD";
                }
            }
            return "TBD";
        }

        private void GenerateSetIdAndLoadDBCode(string fieldName, string fieldType)
        {
            StringBuilder sb = new StringBuilder();

            string methodName = "Set" + ConvertToTitleCase(fieldName) + "AndLoadDB";
            string parameterType = GetCSharpParameterType(fieldType);
            string exceptionMessage = fieldName + " tidak ditemukan";

            sb.AppendLine($"//-------------------------------------------------------------------------");
            sb.AppendLine($"void MSupplierEntity::{methodName}(const {parameterType} {fieldName})");
            sb.AppendLine($"{{");
            sb.AppendLine($"   std::map <AnsiString, Variant> mKeyValue;");
            sb.AppendLine($"   mKeyValue[\"{fieldName}\"] = {fieldName};");
            sb.AppendLine($"   if(!loadById(mKeyValue))");
            sb.AppendLine($"      throw Exception(\"{exceptionMessage}\");");
            sb.AppendLine($"}}");
            sb.AppendLine($"//-------------------------------------------------------------------------");
        }

        private string ConvertToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string[] words = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();

            foreach (string word in words)
            {
                sb.Append(char.ToUpper(word[0]));
                sb.Append(word.Substring(1).ToLower());
            }

            return sb.ToString();
        }

        private string GetCSharpParameterType(string fieldType)
        {
            switch (fieldType)
            {
                case "int":
                    return "int";
                case "string":
                    return "string";
                case "DateTime":
                    return "DateTime";
                default:
                    return "TBD";
            }
        }

        private void GenerateLoadByIdCode(string entityName, Dictionary<string, string> fields)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"const bool {entityName}::loadById(const std::map<AnsiString, Variant>& _mKeyIds)");
            sb.AppendLine($"{{");
            sb.AppendLine($"   try");
            sb.AppendLine($"   {{");
            sb.AppendLine($"      generateQueryAndParamFromId(\"{entityName}\", _mKeyIds);");
            sb.AppendLine($"");
            sb.AppendLine($"      TUniQuery& uniquery = getUniQuery();");
            sb.AppendLine($"      uniquery.Open();");

            foreach (var field in fields)
            {
                string fieldName = field.Key;
                string fieldType = field.Value;
                string cppFieldType = GetCppFieldType(fieldType);

                sb.AppendLine($"      {fieldName} = uniquery.FieldByName(\"{fieldName}\")->As{cppFieldType};");
            }

            sb.AppendLine($"");
            sb.AppendLine($"      return true;");
            sb.AppendLine($"   }}");
            sb.AppendLine($"   catch (Exception& e)");
            sb.AppendLine($"   {{");
            sb.AppendLine($"      setErrorMessage(\"error find by id {entityName} entity \" + e.Message);");
            sb.AppendLine($"   }}");
            sb.AppendLine($"   return false;");
            sb.AppendLine($"}}");
            sb.AppendLine($"//-------------------------------------------------------------------------");
        }

        private string GetCppFieldType(string fieldType)
        {
            switch (fieldType)
            {
                case "int":
                    return "Integer";
                case "string":
                    return "String";
                case "float":
                    return "Float";
                case "DateTime":
                    return "DateTime";
                default:
                    return "TBD";
            }
        }

        private void GenerateLoadDataFromDatasetCode(string entityName, Dictionary<string, string> fields)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"const bool {entityName}::loadDataFromDataset(TDataSet* const _dataSet)");
            sb.AppendLine($"{{");
            sb.AppendLine($"   try");
            sb.AppendLine($"   {{");
            sb.AppendLine($"      if (_dataSet != 0 && !_dataSet->IsEmpty())");
            sb.AppendLine($"      {{");

            foreach (var field in fields)
            {
                string fieldName = field.Key;
                string fieldType = field.Value;
                string cppFieldType = GetCppFieldType(fieldType);

                sb.AppendLine($"         {fieldName} = _dataSet->FieldByName(\"{fieldName}\")->As{cppFieldType};");
            }

            sb.AppendLine($"");
            sb.AppendLine($"         return true;");
            sb.AppendLine($"      }}");
            sb.AppendLine($"   }}");
            sb.AppendLine($"   catch (Exception& e)");
            sb.AppendLine($"   {{");
            sb.AppendLine($"      setErrorMessage(\"Gagal load {entityName} dari dataset \" + e.Message);");
            sb.AppendLine($"   }}");
            sb.AppendLine($"   return false;");
            sb.AppendLine($"}}");
        }

        private void GenerateIsNewCode(string entityName, Dictionary<string, string> fields)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"const bool {entityName}::isNew() const");
            sb.AppendLine($"{{");
            sb.AppendLine($"   return {fields["idMSupplier"]} <= 0;");
            sb.AppendLine($"}}");
            sb.AppendLine($"//-------------------------------------------------------------------------");
        }

        private void GenerateMarkAsNewCode(string entityName, Dictionary<string, string> fields)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"const void {entityName}::markAsNew()");
            sb.AppendLine($"{{");
            sb.AppendLine($"   {fields["idMSupplier"]} = 0;");
            sb.AppendLine($"}}");
            sb.AppendLine($"//-------------------------------------------------------------------------");
        }

        private void GeneratePrimaryKeyCode(string entityName, Dictionary<string, string> fields)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"const AnsiString {entityName}::primaryKey() const");
            sb.AppendLine($"{{");
            sb.AppendLine($"   return \"idMSupplier\";");
            sb.AppendLine($"}}");
        }

        private string GenerateTambahRecordCode(params string[] fields)
        {
            string code = "public bool tambahRecord()\n";
            code += "{\n";
            code += "    try\n";
            code += "    {\n";
            code += "        TUniQuery uniquery = getUniQuery();\n";
            code += "        uniquery.SQL.Text = \"CALL IsiMSupplier(\" +\n";

            string paramsText = string.Join(", ", Array.ConvertAll(fields, field => $":v{field.ToUpper()}"));
            code += paramsText;
            code += ");\";\n";

            foreach (var field in fields)
            {
                code += $"        uniquery.Params.ParamByName(\"v{field.ToUpper()}\").AsString = {field};\n";
            }

            code += "        uniquery.ExecSQL();\n";
            code += "        uniquery.SQL.Text = \"SELECT MAX(IdMSupplier) as last_id, MAX(timeupdate) as timeupdate from msupplier \";\n";
            code += "        uniquery.Open();\n";
            code += "\n";
            code += "        if (uniquery.RecordCount > 0)\n";
            code += "        {\n";
            code += "            idMSupplier = uniquery.FieldByName(\"last_id\").AsInteger;\n";
            code += "            timeUpdate = uniquery.FieldByName(\"timeupdate\").AsDateTime;\n";
            code += "        }\n";
            code += "\n";
            code += "        return true;\n";
            code += "    }\n";
            code += "    catch (Exception e)\n";
            code += "    {\n";
            code += "        setErrorMessage(\"Gagal isi mSupplier \" + e.Message);\n";
            code += "    }\n";
            code += "    return false;\n";
            code += "}\n";

            return code;
        }

        private string GenerateEditRecordCode(string methodName, string cppCode)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"const bool {methodName}()");
            sb.AppendLine("{");
            sb.AppendLine("   try");
            sb.AppendLine("   {");
            sb.AppendLine("      TUniQuery& uniquery = getUniQuery();");
            sb.AppendLine($"      uniquery.SQL->Text = \"{cppCode}\";");
            sb.AppendLine();
            sb.AppendLine("      // Set query parameters");
            sb.AppendLine("      // uniquery.Params.ParamByName(...)");
            sb.AppendLine();
            sb.AppendLine("      uniquery.ExecSQL();");
            sb.AppendLine();
            sb.AppendLine("      return true;");
            sb.AppendLine("   }");
            sb.AppendLine("   catch(const Exception& e)");
            sb.AppendLine("   {");
            sb.AppendLine("      setErrorMessage(\"Gagal " + methodName + " \" + e.Message);");
            sb.AppendLine("   }");
            sb.AppendLine("   return false;");
            sb.AppendLine("}");

            return sb.ToString();
        }
        private string GenerateValidationCode(string methodName, string cppCode)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"const bool {methodName}()");
            sb.AppendLine("{");
            sb.AppendLine("   bool isValid = validateKodeSupplier() &&");
            sb.AppendLine("                  validateNamaSupplier() &&");
            sb.AppendLine("                  validateAlamat() &&");
            sb.AppendLine("                  validateKota() &&");
            sb.AppendLine("                  validateNoTelepon();");
            sb.AppendLine();
            sb.AppendLine("   if(isValid)");
            sb.AppendLine("   {");
            sb.AppendLine("      setErrorMessage(\"\");");
            sb.AppendLine("   }");
            sb.AppendLine("   return isValid;");
            sb.AppendLine("}");
            sb.AppendLine("//-------------------------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine($"const bool {methodName.Replace("isValidToPersist", "validateKodeSupplier")}()");
            sb.AppendLine("{");
            sb.AppendLine("   if(kodeSupplier == \"\")");
            sb.AppendLine("   {");
            sb.AppendLine("      setErrorMessage(\"Kode Supplier tidak boleh kosong\");");
            sb.AppendLine("      return false;");
            sb.AppendLine("   }");
            sb.AppendLine("   else");
            sb.AppendLine("   {");
            sb.AppendLine("      TUniQuery& uniquery = getUniQuery();");
            sb.AppendLine("      uniquery.Close();");
            sb.AppendLine($"      uniquery.SQL->Text = \"{cppCode}\";");
            sb.AppendLine();
            sb.AppendLine("      uniquery.Params->ParamByName(\"kodesupplier\")->AsString = kodeSupplier;");
            sb.AppendLine("      uniquery.Open();");
            sb.AppendLine();
            sb.AppendLine("      if(uniquery.RecordCount > 0)");
            sb.AppendLine("      {");
            sb.AppendLine("         setErrorMessage(\"Kode Supplier \" + kodeSupplier + \" sudah ada\");");
            sb.AppendLine("         return false;");
            sb.AppendLine("      }");
            sb.AppendLine("   }");
            sb.AppendLine();
            sb.AppendLine("   return true;");
            sb.AppendLine("}");
            sb.AppendLine("//-------------------------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine($"const bool {methodName.Replace("isValidToPersist", "validateNamaSupplier")}()");
            sb.AppendLine("{");
            sb.AppendLine("   if(namaSupplier == \"\")");
            sb.AppendLine("   {");
            sb.AppendLine("      setErrorMessage(\"Nama Supplier tidak boleh kosong\");");
            sb.AppendLine("      return false;");
            sb.AppendLine("   }");
            sb.AppendLine();
            sb.AppendLine("   return true;");
            sb.AppendLine("}");
            sb.AppendLine("//-------------------------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine($"const bool {methodName.Replace("isValidToPersist", "validateAlamat")}()");
            sb.AppendLine("{");
            sb.AppendLine("   if(alamat == \"\")");
            sb.AppendLine("   {");
            sb.AppendLine("      setErrorMessage(\"Alamat tidak boleh kosong\");");
            sb.AppendLine("      return false;");
            sb.AppendLine("   }");
            sb.AppendLine();
            sb.AppendLine("   return true;");
            sb.AppendLine("}");
            sb.AppendLine("//-------------------------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine($"const bool {methodName.Replace("isValidToPersist", "validateKota")}()");
            sb.AppendLine("{");
            sb.AppendLine("   if(kota == \"\")");
            sb.AppendLine("   {");
            sb.AppendLine("      setErrorMessage(\"Kota tidak boleh kosong\");");
            sb.AppendLine("      return false;");
            sb.AppendLine("   }");
            sb.AppendLine();
            sb.AppendLine("   return true;");
            sb.AppendLine("}");
            sb.AppendLine("//-------------------------------------------------------------------------");
            sb.AppendLine();
            sb.AppendLine($"const bool {methodName.Replace("isValidToPersist", "validateNoTelepon")}()");
            sb.AppendLine("{");
            sb.AppendLine("   if(noTelepon == \"\")");
            sb.AppendLine("   {");
            sb.AppendLine("      setErrorMessage(\"No Telepon tidak boleh kosong\");");
            sb.AppendLine("      return false;");
            sb.AppendLine("   }");
            sb.AppendLine();
            sb.AppendLine("   return true;");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateHydrateCode(string methodName, string cppCode)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"void MSupplierEntity::{methodName}(TJSONObject* const _jsonObject)");
            sb.AppendLine("{");

            string[] lines = cppCode.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { '=', '>', ';', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string variable = parts[0].Trim();
                    string value = parts[1].Trim();

                    sb.AppendLine($"   {variable} = ((TJSONValue*)_jsonObject->GetValue(\"{value}\"))->Value();");
                }
            }

            sb.AppendLine("   idMUserUpdate = jsonParser.getValueInt(_jsonObject, \"IdMUserUpdate\");");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
