using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemarCPPCodeGenerator.Services
{
    public class HeaderService
    {
        public string GenerateHeader(string entityName, string fieldsText)
        {

            string[] fields = fieldsText.Split(';');

            string code = "class " + entityName + "Entity : public BaseEntity\n";
            code += "{\n";
            code += "private:\n";

            foreach (string field in fields)
            {
                string[] parts = field.Split(':');
                string fieldName = parts[0].Trim();
                string dataType = parts[1].Trim();

                code += "   " + dataType + " " + fieldName + ";\n";
            }

            code += "\n";
            code += "private:\n";
            code += "   " + entityName + "Entity(const " + entityName + "Entity& _" + entityName + "Entity);\n";
            code += "   const " + entityName + "Entity& operator=(const " + entityName + "Entity& _" + entityName + "Entity);\n";
            code += "\n";
            code += "public:\n";
            code += "   " + entityName + "Entity();\n";
            code += "   virtual const bool operator==(const BaseEntity& _baseEntity) const;\n";
            code += "   const bool operator==(const " + entityName + "Entity& _" + entityName + "Entity) const;\n";

            foreach (string field in fields)
            {
                string[] parts = field.Split(':');
                string fieldName = parts[0].Trim();
                string dataType = parts[1].Trim();

                code += "\n";
                code += "   inline const " + dataType + " get" + fieldName + "() const { return " + fieldName + "; }\n";
                code += "   inline void set" + fieldName + "(const " + dataType + "& _" + fieldName + ") { " + fieldName + " = _" + fieldName + "; }\n";
            }

            code += "\n";
            code += "   virtual const DictionaryVTFieldAndValue getId() const;\n";
            code += "   virtual const std::map<AnsiString, Variant> getAllMapValue() const;\n";
            code += "\n";
            code += "   // crud\n";
            code += "   virtual const bool tambahRecord();\n";
            code += "   virtual const bool editRecord();\n";
            code += "   virtual const bool deleteRecord();\n";
            code += "\n";
            code += "   virtual const bool loadById(const std::map<AnsiString, Variant>& _mKeyIds);\n";
            code += "   virtual const bool loadDataFromDataset(TDataSet* const _dataSet);\n";
            code += "   virtual const bool isNew() const;\n";
            code += "   virtual const void markAsNew();\n";
            code += "   virtual const AnsiString primaryKey() const;\n";
            code += "   virtual void hydrate(TJSONObject* const _jsonObject);\n";
            code += "\n";
            code += "   // validation\n";
            code += "   virtual const bool isValidToPersist();\n";

            foreach (string field in fields)
            {
                string[] parts = field.Split(':');
                string fieldName = parts[0].Trim();
                string dataType = parts[1].Trim();

                code += "   const bool validate" + fieldName + "();\n";
            }

            code += "};\n";

            return code;
        }
    }
}
