using System.Collections;
using System.IO;

namespace System.Xml
{
	public class XmlNodeWriter : XmlWriter
	{
		private XmlNode root;

		private XmlNode current;

		private XmlDocument owner;

		private XmlAttribute ca;

		private XmlNameTable nameTable;

		private WriteState state;

		private string xmlnsURI;

		private string xmlns;

		public override WriteState WriteState => state switch
		{
			WriteState.Start => WriteState.Start, 
			WriteState.Prolog => WriteState.Prolog, 
			WriteState.Element => WriteState.Element, 
			WriteState.Attribute => WriteState.Attribute, 
			WriteState.Content => WriteState.Content, 
			WriteState.Closed => WriteState.Closed, 
			_ => WriteState.Closed, 
		};

		public override string XmlLang
		{
			get
			{
				for (XmlNode parentNode = current; parentNode != null; parentNode = parentNode.ParentNode)
				{
					if (parentNode is XmlElement xmlElement)
					{
						string attribute = xmlElement.GetAttribute("lang", "http://www.w3.org/XML/1998/namespace");
						if (attribute != null && attribute != string.Empty)
						{
							return attribute;
						}
					}
				}
				return null;
			}
		}

		public override XmlSpace XmlSpace
		{
			get
			{
				for (XmlNode parentNode = current; parentNode != null; parentNode = parentNode.ParentNode)
				{
					if (parentNode is XmlElement xmlElement)
					{
						string attribute = xmlElement.GetAttribute("space", "http://www.w3.org/XML/1998/namespace");
						if (attribute == "default")
						{
							return XmlSpace.Default;
						}
						if (attribute == "preserve")
						{
							return XmlSpace.Preserve;
						}
					}
				}
				return XmlSpace.None;
			}
		}

		private void Init(XmlNode root, bool clearCurrentContents)
		{
			this.root = root;
			if (clearCurrentContents)
			{
				this.root.RemoveAll();
			}
			if (root is XmlDocument)
			{
				owner = (XmlDocument)root;
				state = WriteState.Start;
			}
			else
			{
				owner = root.OwnerDocument;
				state = WriteState.Content;
			}
			current = root;
			nameTable = owner.NameTable;
			xmlnsURI = nameTable.Add("http://www.w3.org/2000/xmlns/");
			xmlns = nameTable.Add("xmlns");
		}

		public XmlNodeWriter(XmlElement root, bool clearCurrentContents)
		{
			Init(root, clearCurrentContents);
		}

		public XmlNodeWriter(XmlDocument root, bool clearCurrentContents)
		{
			Init(root, clearCurrentContents);
		}

		public override void Close()
		{
			current = root;
			state = WriteState.Closed;
		}

		public override void Flush()
		{
		}

		public override string LookupPrefix(string namespaceURI)
		{
			namespaceURI = nameTable.Add(namespaceURI);
			if ((object)namespaceURI == xmlnsURI)
			{
				return xmlns;
			}
			if (current != null)
			{
				XmlNode xmlNode = current;
				while (xmlNode != null)
				{
					if (xmlNode.NodeType == XmlNodeType.Element)
					{
						if ((object)xmlNode.NamespaceURI == namespaceURI)
						{
							return xmlNode.Prefix;
						}
						XmlElement xmlElement = (XmlElement)xmlNode;
						if (xmlElement.HasAttributes)
						{
							int count = xmlElement.Attributes.Count;
							for (int num = count - 1; num >= 0; num--)
							{
								XmlNode xmlNode2 = xmlElement.Attributes[num];
								if ((object)xmlNode2.Prefix == xmlns && xmlNode2.Value == namespaceURI)
								{
									return xmlNode2.LocalName;
								}
							}
						}
						xmlNode = xmlNode.ParentNode;
					}
					else
					{
						xmlNode = ((xmlNode.NodeType != XmlNodeType.Attribute) ? xmlNode.ParentNode : ((XmlAttribute)xmlNode).OwnerElement);
					}
				}
			}
			return null;
		}

		public override void WriteBase64(byte[] buffer, int index, int count)
		{
			WriteString(Convert.ToBase64String(buffer, index, count));
		}

		public override void WriteBinHex(byte[] buffer, int index, int count)
		{
			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
			xmlTextWriter.WriteBinHex(buffer, index, count);
			xmlTextWriter.Close();
			WriteString(stringWriter.ToString());
		}

		public override void WriteCData(string text)
		{
			if (state == WriteState.Attribute || state == WriteState.Element)
			{
				state = WriteState.Content;
			}
			if (state != WriteState.Content)
			{
				throw new InvalidOperationException("Writer is in the state '" + WriteState.ToString() + "' which is not valid for writing CData elements");
			}
			current.AppendChild(owner.CreateCDataSection(text));
		}

		public override void WriteCharEntity(char ch)
		{
			WriteString(Convert.ToString(ch));
		}

		public override void WriteChars(char[] buffer, int index, int count)
		{
			WriteString(new string(buffer, index, count));
		}

		public override void WriteComment(string text)
		{
			if (state == WriteState.Attribute || state == WriteState.Element)
			{
				state = WriteState.Content;
			}
			if (state != WriteState.Content && state != WriteState.Prolog && state != 0)
			{
				throw new InvalidOperationException("Writer is in the state '" + WriteState.ToString() + "' which is not valid for writing comments");
			}
			current.AppendChild(owner.CreateComment(text));
			if (state == WriteState.Start)
			{
				state = WriteState.Prolog;
			}
		}

		public override void WriteDocType(string name, string pubid, string sysid, string subset)
		{
			if (state != WriteState.Prolog && state != 0)
			{
				throw new InvalidOperationException("Writer is not in the Start or Prolog state, or root node is not an XmlDocument object");
			}
			if (owner.DocumentType != null)
			{
				owner.RemoveChild(owner.DocumentType);
			}
			owner.XmlResolver = null;
			current.AppendChild(owner.CreateDocumentType(name, pubid, sysid, subset));
			state = WriteState.Prolog;
		}

		public override void WriteEndAttribute()
		{
			if (state != WriteState.Attribute)
			{
				throw new InvalidOperationException("Writer is not in the Attribute state");
			}
			state = WriteState.Element;
		}

		public override void WriteEndDocument()
		{
			current = root;
			state = WriteState.Start;
		}

		public override void WriteEndElement()
		{
			if (current == root)
			{
				throw new InvalidOperationException("Too many WriteEndElement calls have been made");
			}
			current = current.ParentNode;
			state = WriteState.Content;
		}

		public override void WriteEntityRef(string name)
		{
			if (state == WriteState.Element)
			{
				state = WriteState.Content;
			}
			XmlNode xmlNode = current;
			if (state == WriteState.Attribute)
			{
				xmlNode = ca;
			}
			else if (state != WriteState.Content)
			{
				throw new InvalidOperationException("Invalid state '" + WriteState.ToString() + "' for entity reference");
			}
			xmlNode.AppendChild(owner.CreateEntityReference(name));
		}

		public override void WriteFullEndElement()
		{
			WriteEndElement();
		}

		public override void WriteName(string name)
		{
			WriteString(XmlConvert.VerifyName(name));
		}

		public override void WriteNmToken(string name)
		{
			string text = XmlConvert.VerifyName("a" + name);
			WriteString(text.Substring(1));
		}

		public override void WriteProcessingInstruction(string name, string text)
		{
			if (state == WriteState.Attribute || state == WriteState.Element)
			{
				state = WriteState.Content;
			}
			if (state != WriteState.Content && state != WriteState.Prolog && state != 0)
			{
				throw new InvalidOperationException("Writer is in the state '" + WriteState.ToString() + "' which is not valid for writing processing instructions");
			}
			if (name == "xml")
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.InnerXml = "<?xml " + text + "?><root/>";
				current.AppendChild(owner.ImportNode(xmlDocument.FirstChild, deep: true));
			}
			else
			{
				current.AppendChild(owner.CreateProcessingInstruction(name, text));
			}
			if (state == WriteState.Start)
			{
				state = WriteState.Prolog;
			}
		}

		public override void WriteQualifiedName(string localName, string ns)
		{
			string text = LookupPrefix(ns);
			if (text == null)
			{
				throw new InvalidOperationException("Namespace '" + ns + "' is not in scope");
			}
			if (text == string.Empty)
			{
				WriteString(localName);
			}
			else
			{
				WriteString(text + ":" + localName);
			}
		}

		public override void WriteRaw(string data)
		{
			switch (state)
			{
			case WriteState.Element:
				state = WriteState.Content;
				goto case WriteState.Start;
			case WriteState.Attribute:
			{
				ArrayList arrayList = new ArrayList();
				if (ca.HasChildNodes)
				{
					while (ca.FirstChild != null)
					{
						arrayList.Add(ca.FirstChild);
						ca.RemoveChild(ca.FirstChild);
					}
				}
				ca.InnerXml = data;
				for (int num = arrayList.Count - 1; num >= 0; num--)
				{
					ca.PrependChild((XmlNode)arrayList[num]);
				}
				break;
			}
			case WriteState.Start:
			case WriteState.Prolog:
			case WriteState.Content:
			{
				ArrayList arrayList2 = new ArrayList();
				if (current.HasChildNodes)
				{
					while (current.FirstChild != null)
					{
						arrayList2.Add(current.FirstChild);
						current.RemoveChild(current.FirstChild);
					}
				}
				current.InnerXml = data;
				for (int num2 = arrayList2.Count - 1; num2 >= 0; num2--)
				{
					current.PrependChild((XmlNode)arrayList2[num2]);
				}
				state = WriteState.Content;
				break;
			}
			case WriteState.Closed:
				throw new InvalidOperationException("Writer is closed");
			}
		}

		public override void WriteRaw(char[] buffer, int index, int count)
		{
			WriteRaw(new string(buffer, index, count));
		}

		public override void WriteStartAttribute(string prefix, string localName, string ns)
		{
			if (prefix == xmlns)
			{
				if (ns == null)
				{
					ns = xmlnsURI;
				}
			}
			else if ((prefix == null || prefix.Length == 0) && localName == xmlns)
			{
				if (ns == null)
				{
					ns = xmlnsURI;
				}
				prefix = "";
			}
			else if (prefix == null && ns != null && ns.Length > 0)
			{
				prefix = LookupPrefix(ns);
			}
			if (state == WriteState.Attribute)
			{
				state = WriteState.Element;
			}
			if (state != WriteState.Element)
			{
				throw new InvalidOperationException("Writer is not in a start tag, so it cannot write attributes.");
			}
			ca = owner.CreateAttribute(prefix, localName, ns);
			current.Attributes.Append(ca);
			state = WriteState.Attribute;
		}

		public override void WriteStartDocument()
		{
			if (state != 0)
			{
				throw new InvalidOperationException("Writer is not in the Start state or root node is not an XmlDocument object");
			}
			current.AppendChild(owner.CreateXmlDeclaration("1.0", null, null));
			state = WriteState.Prolog;
		}

		public override void WriteStartDocument(bool standalone)
		{
			if (state != 0)
			{
				throw new InvalidOperationException("Writer is not in the Start state or root node is not an XmlDocument object");
			}
			current.AppendChild(owner.CreateXmlDeclaration("1.0", null, standalone ? "yes" : "no"));
			state = WriteState.Prolog;
		}

		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			if (state == WriteState.Attribute || state == WriteState.Element || state == WriteState.Start || state == WriteState.Prolog)
			{
				state = WriteState.Content;
			}
			if (state != WriteState.Content)
			{
				throw new InvalidOperationException("Writer is in the wrong state for writing element content");
			}
			if (prefix == null && ns != null && ns.Length > 0)
			{
				prefix = LookupPrefix(ns);
			}
			XmlElement newChild = owner.CreateElement(prefix, localName, ns);
			current.AppendChild(newChild);
			current = newChild;
			state = WriteState.Element;
		}

		public override void WriteString(string text)
		{
			XmlNode xmlNode = current;
			if (state == WriteState.Attribute)
			{
				xmlNode = ca;
			}
			else if (state == WriteState.Element)
			{
				state = WriteState.Content;
			}
			if (state != WriteState.Attribute && state != WriteState.Content)
			{
				throw new InvalidOperationException("Writer is in the wrong state to be writing text content");
			}
			XmlNode lastChild = xmlNode.LastChild;
			if (lastChild == null || !(lastChild is XmlText))
			{
				lastChild = owner.CreateTextNode(text);
				xmlNode.AppendChild(lastChild);
			}
			else
			{
				XmlText xmlText = lastChild as XmlText;
				xmlText.AppendData(text);
			}
		}

		public override void WriteSurrogateCharEntity(char lowChar, char highChar)
		{
			WriteString(new string(new char[2] { lowChar, highChar }));
		}

		public override void WriteWhitespace(string ws)
		{
			if (state == WriteState.Attribute || state == WriteState.Element)
			{
				state = WriteState.Content;
			}
			if (state != WriteState.Content && state != WriteState.Prolog && state != 0)
			{
				throw new InvalidOperationException("Writer is not in the right state to be writing whitespace nodes");
			}
			current.AppendChild(owner.CreateWhitespace(ws));
			if (state == WriteState.Start)
			{
				state = WriteState.Prolog;
			}
		}
	}
}
