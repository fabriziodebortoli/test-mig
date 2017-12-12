using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Permissions;
using System.Xml;
//
using Microsoft.Web.Services.Security;


namespace Microarea.TaskBuilderNet.Licence.Activation
{
	[SecurityPermission(SecurityAction.Demand,
		 Flags= SecurityPermissionFlag.UnmanagedCode)]
	// ========================================================================
	public class SourceControlToken : BinarySecurityToken/*MicroareaAsymmetricKey*/
	{
		private	Components.GenericMicroareaComponents	gen;
		public  static readonly string NamespaceURI =
			"http://www.Microarea.it/SourceControlToken/";
		public  static readonly XmlQualifiedName TokenValueType =
			new XmlQualifiedName("SourceControlToken", NamespaceURI);

		private RSACryptoServiceProvider rsa;

		public override bool SupportsDigitalSignature	{ get { return (rsa != null); } }
		public override bool SupportsDataEncryption		{ get { return (rsa != null); } }
		public Boolean PrivateKeyAvailable { get { return true;} }

		// --------------------------------------------------------------------
		public SourceControlToken() : base(TokenValueType)
		{
			gen = new Microarea.Library.Activation.Components.GenericMicroareaComponents();
			CspParameters cspParam = new CspParameters();
			cspParam.Flags = CspProviderFlags.UseMachineKeyStore;
			cspParam.KeyContainerName = "SourceControlToken";

			rsa = new RSACryptoServiceProvider(cspParam);

			rsa.FromXmlString(gen.GetValue);

			if (rsa != null)
			{
				string rsaString = rsa.ToXmlString(false);
				RawData = System.Text.Encoding.UTF8.GetBytes(rsaString);
			}

		}

		// --------------------------------------------------------------------
		public SourceControlToken(XmlElement element) : base(element)
		{}

		// --------------------------------------------------------------------
		public override SignatureKey SignatureKey
		{
			get
			{
				if (rsa == null )
					throw new InvalidOperationException
						("Signature key is unavailable.");
				
				return new SignatureKey(rsa);
			}
		}

		// --------------------------------------------------------------------
		public override AuthenticationKey AuthenticationKey
		{
			get
			{
				if (rsa == null)
					throw new InvalidOperationException
						("Authentication key is unavailable.");

				return new AuthenticationKey(rsa);
			}
		}

		// --------------------------------------------------------------------
		public override DecryptionKey DecryptionKey
		{
			get
			{
				if (rsa == null)
					throw new InvalidOperationException
						("Decryption key is unavailable.");
				
				return new AsymmetricDecryptionKey(rsa);
			}
		}

		// --------------------------------------------------------------------
		public override EncryptionKey EncryptionKey
		{
			get
			{
				if (rsa == null)
					throw new InvalidOperationException
						("Encryption key is unavailable.");

				EncryptionKey key = new	AsymmetricEncryptionKey(rsa);
				KeyInfoName name = new KeyInfoName();
				name.Value = "SourceControlToken";
				key.KeyInfo.AddClause(name);

				return key;
			}
		}

		// --------------------------------------------------------------------
		public override void Verify(){}

		// --------------------------------------------------------------------
		public string GetXmlString()
		{
			return rsa.ToXmlString(false);
		}

		// --------------------------------------------------------------------
		public override void LoadXml(XmlElement element)
		{
			base.LoadXml(element);

			if (RawData != null)
			{
				string rsaString = System.Text.Encoding.UTF8.GetString(RawData);
				rsa = new RSACryptoServiceProvider();
				rsa.FromXmlString(rsaString);
			}
		}
	}


	[SecurityPermission(SecurityAction.Demand,
		 Flags= SecurityPermissionFlag.UnmanagedCode)]
	// ========================================================================
	public class SourceControlTokenBinaryDecryptionProvider : DecryptionKeyProvider
	{
		// --------------------------------------------------------------------
		public override DecryptionKey GetDecryptionKey
			(string algorithmUri, KeyInfo keyInfo)
		{
			if ( null == keyInfo )
				throw new ArgumentNullException("keyInfo");

			Components.GenericMicroareaComponents gen =
				new Microarea.Library.Activation.Components.GenericMicroareaComponents();
			RSACryptoServiceProvider halg = new RSACryptoServiceProvider();
			halg.FromXmlString(gen.GetValue);

			return new AsymmetricDecryptionKey(halg);
		}
	}
}