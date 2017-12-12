
namespace Microarea.TaskBuilderNet.Core.CoreTypes
{
	//=========================================================================
	// Valori inseriti da 'TaskBuilder\Framework\TbXmlCore\XMLSchema.h'
	public sealed class XsdSchemaHelper
	{
		public const string SchemaXsdNamespaceUriValue = "http://www.w3.org/2001/XMLSchema";
		public const string SchemaXsdDataUriValue = "urn:schemas-microsoft-com:xml-msdata";
		public const string SchemaXsdNamespaceUriPrefix = "xs";
		public const string SchemaXsdNamespaceUriPrefixEx = SchemaXsdNamespaceUriPrefix + ":";

		public const string SchemaXsdSchemaTag = "schema";
		public const string SchemaXsdXmlns = "xmlns";
		public const string SchemaXsdId = "id";
		public const string SchemaXsdDataUriAttribute = SchemaXsdXmlns + ":msdata";

		public const string SchemaXsdElementFormDefaultAttribute = "elementFormDefault";
		public const string SchemaXsdAttributeFormDefaultAttribute = "attributeFormDefault";

		public const string SchemaXsdQualifiedValue = "qualified";
		public const string SchemaXsdUnqualifiedValue = "unqualified";

		public const string SchemaXsdUseAttribute = "use";
		public const string SchemaXsdFixedAttribute = "fixed";
		public const string SchemaXsdDefaultAttribute = "default";

		public const string SchemaXsdOptionalValue = "optional";
		public const string SchemaXsdProhibitedValue = "prohibited";
		public const string SchemaXsdRequiredValue = "required";

		public const string SchemaXsdElementTag = "element";
		public const string SchemaXsdAttributeTag = "attribute";
		public const string SchemaXsdComplexTypeTag = "complexType";
		public const string SchemaXsdSimpleTypeTag = "simpleType";
		public const string SchemaXsdRestrictionTag = "restriction";
		public const string SchemaXsdEnumerationTag = "enumeration";
		public const string SchemaXsdUnionTag = "union";

		public const string SchemaXsdExtensionTag = "extension";

		public const string SchemaXsdMaxLengthTag = "maxLength";

		public const string SchemaXsdMinOccursTag = "minOccurs";
		public const string SchemaXsdMaxOccursTag = "maxOccurs";
		public const string SchemaXsdUnboundedValue = "unbounded";

		public const string SchemaXsdTargetNamespaceAttribute = "targetNamespace";
		public const string SchemaXsdNameAttribute = "name";
		public const string SchemaXsdTypeAttribute = "type";
		public const string SchemaXsdBaseAttribute = "base";
		public const string SchemaXsdValueAttribute = "value";
		public const string SchemaXsdMemberTypesAttribute = "memberTypes";

		public const string SchemaXsdTypeAll = "all";
		public const string SchemaXsdTypeSequence = "sequence";
		public const string SchemaXsdTypeChoice = "choice";
		public const string SchemaXsdTypeGroup = "group";
		public const string SchemaXsdTypeComplexContent = "complexContent";
		public const string SchemaXsdTypeSimpleContent = "simpleContent";

		public const string SchemaDataTypeStringValue = "string";
		public const string SchemaDataTypeIntegerValue = "integer"; //da chiedere a PERASSO

		public const string SchemaDataTypeFloatValue = "float"; //da chiedere a PERASSO
		public const string SchemaDataTypeDateValue = "date";
		public const string SchemaDataTypeDateTimeValue = "dateTime";
		public const string SchemaDataTypeTimeValue = "time";
		public const string SchemaDataTypeBooleanValue = "boolean";

		//@@BAUZI
		public const string SchemaDataTypeShortValue = "short";
		public const string SchemaDataTypeIntValue = "int"; //per i DataInt
		public const string SchemaDataTypeLongValue = "long"; //per i DataLng
		public const string SchemaDataTypeDoubleValue = "double"; //per i DataDbl e derivati
		public const string SchemaDataTypeUintValue = "unsignedInt"; //per gli enumerativi


		public const string SchemaXsdDataTypeStringValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeStringValue;
		public const string SchemaXsdDataTypeIntegerValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeIntegerValue;
		public const string SchemaXsdDataTypeFloatValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeFloatValue;
		public const string SchemaXsdDataTypeDateValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeDateValue;
		public const string SchemaXsdDataTypeDateTimeValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeDateTimeValue;
		public const string SchemaXsdDataTypeTimeValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeTimeValue;
		public const string SchemaXsdDataTypeBooleanValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeBooleanValue;

		//@@BAUZI
		public const string SchemaXsdDataTypeShortValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeShortValue;
		public const string SchemaXsdDataTypeIntValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeIntValue;
		public const string SchemaXsdDataTypeLongValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeLongValue;
		public const string SchemaXsdDataTypeDoubleValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeDoubleValue;
		public const string SchemaXsdDataTypeUintValue = SchemaXsdNamespaceUriPrefixEx + SchemaDataTypeUintValue;

		//---------------------------------------------------------------------
		private XsdSchemaHelper()
		{ }
	}
}
