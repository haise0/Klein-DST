﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DSTEd.Core;
using DSTEd.Core.IO;

namespace DSTEd.UI.Components {
	public partial class WorkspaceTree : UserControl
	{
		private Func<FileNode, TreeViewItem> callback = null;

		public WorkspaceTree(FileSystem files, Func<FileNode, TreeViewItem> callback)
		{
			InitializeComponent();

			this.callback = callback;
			this.tree.Items.Clear();

			files.GetDirectories(delegate (List<FileNode> f)
			{
				foreach (FileNode directory in f)
				{
					Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(delegate ()
					{
						this.Render(directory, this.tree);
					}));
				}
			});
		}

		private TreeViewItem Render(FileNode files, TreeView container)
		{
			TreeViewItem item = new TreeViewItem { Header = files.GetName() };
			item.FontWeight = FontWeights.Normal;

			if (files.GetName().StartsWith("workshop-"))
			{
				item = callback?.Invoke(files);
			}

			if (files.HasSubdirectories())
			{
				foreach (FileNode directory in files.GetSubdirectories())
				{
					TreeViewItem root = this.Render(directory, null);
					root.FontWeight = FontWeights.Normal;

					if (container != null)
					{
						container.Items.Add(root);
					}
					else
					{
						item.Items.Add(root);
					}
				}
			}

			if (files.HasFiles())
			{
				Dictionary<string, Dictionary<string, TreeViewItem>> texture_bundles = new Dictionary<string, Dictionary<string, TreeViewItem>>();

				foreach (FileInfo file in files.GetFiles())
				{
					TreeViewItem entry = new TreeViewItem { Header = file.Name };
					Boolean ignore = false;

					entry.MouseRightButtonDown += new MouseButtonEventHandler(delegate (object sender, MouseButtonEventArgs e)
					{
						Logger.Info("ContextMenu: " + file.FullName);
					});

					entry.PreviewMouseDown += new MouseButtonEventHandler(delegate (object sender, MouseButtonEventArgs e)
					{
						Boot.Core().GetWorkspace().OpenDocument(file.FullName);
					});

					if (file.Name == "modinfo.lua")
					{
						entry = new ModInfoItem { Header = "ModInfo" }; // @ToDo need Translation(?)
					}

					// Create texture bundles
					if (file.Name.EndsWith(".tex") || file.Name.EndsWith(".xml"))
					{
						string name = Path.GetFileNameWithoutExtension(file.Name);
						Dictionary<string, TreeViewItem> bundle = null;

						if (!texture_bundles.ContainsKey(name))
						{
							bundle = new Dictionary<string, TreeViewItem>();
							texture_bundles.Add(name, bundle);
						}
						else if (texture_bundles.ContainsKey(name))
						{
							bundle = texture_bundles[name];
						}

						if (file.Name.EndsWith(".tex") && !bundle.ContainsKey("TEXTURE"))
						{
							bundle.Add("TEXTURE", entry);
						}
						else if (file.Name.EndsWith(".xml") && !bundle.ContainsKey("ATLAS"))
						{
							bundle.Add("ATLAS", entry);
						}

						ignore = true;
					}

					if (!ignore)
					{
						item.Items.Add(entry);
					}
				}

				// Build texture bundles
				foreach (KeyValuePair<string, Dictionary<string, TreeViewItem>> bundles in texture_bundles)
				{
					TextureItem entry = new TextureItem { Header = bundles.Key };

					entry.MouseRightButtonDown += new MouseButtonEventHandler(delegate (object sender, MouseButtonEventArgs e)
					{
						Logger.Info("ContextMenu: " + bundles.Key);
					});

					entry.PreviewMouseDown += new MouseButtonEventHandler(delegate (object sender, MouseButtonEventArgs e)
					{
						// @ToDo open Texture-Editor
						Logger.Info("Texture-Editor: " + bundles.Key);
						//this.GetCore().GetWorkspace().OpenDocument(file.FullName);
					});

					foreach (KeyValuePair<string, TreeViewItem> entries in bundles.Value)
					{
						entry.Items.Add(entries.Value);
					}

					item.Items.Add(entry);
				}
			}

			return item;
		}

		private TreeViewItem RenderV2(FileNode files, TreeView container)
		{
			TreeViewItem item;


			if(files.GetName().StartsWith("workshop-"))
			{
				item = new WorkshopItem(files);
			}
			else
			{
				item = new TreeViewItem()
				{
					Header = files.GetName(),
					FontWeight = FontWeights.Normal
				};
			}

			if(files.HasSubdirectories())
			{
				foreach (FileNode dir in files.GetSubdirectories())
				{
					TreeViewItem root = this.RenderV2(dir, null);
					root.FontWeight = FontWeights.Normal;

					if (container != null)
					{
						container.Items.Add(root);
					}
					else
					{
						item.Items.Add(root);
					}
				}
			}

			if(files.HasFiles())
			{
				foreach (FileInfo file in files.GetFiles())
				{
					TreeViewItem entry;
					if(file.Name == "modinfo.lua")
					{
						entry = new ModInfoItem { Header = "ModInfo" };
						item.Items.Add(entry);
						continue;
					}
					if (string.Compare(Path.GetExtension(file.Name), ".xml", true) == 0)
					{
						XmlDocument ktex_atlas = new XmlDocument();
						entry = new TextureItem() { Header = file.Name };
						ktex_atlas.Load(file.FullName);
						try
						{
							foreach (XmlNode texture in ktex_atlas.SelectSingleNode("Atlas").SelectNodes("Texture"))
							{
								foreach (XmlAttribute att in texture.Attributes)
								{
									if (att.LocalName != "filename")
										continue;

								}
							}
						}
						catch (System.Xml.XPath.XPathException)
						{
							continue;
						}
						catch(XmlException)
						{
							continue;
						}
						catch(Exception e)
						{
							Console.WriteLine(e.Message);
							Console.WriteLine(e.StackTrace);
							continue;
						}

						entry.MouseRightButtonDown += new MouseButtonEventHandler(delegate (object sender, MouseButtonEventArgs e)
						{
							Logger.Info("ContextMenu: " + bundles.Key);
						});

						entry.PreviewMouseDown += new MouseButtonEventHandler(delegate (object sender, MouseButtonEventArgs e)
						{
							// @ToDo open Texture-Editor
							Logger.Info("Texture-Editor: " + bundles.Key);
							//this.GetCore().GetWorkspace().OpenDocument(file.FullName);
						});
					}

					/*entry.MouseRightButtonDown += new MouseButtonEventHandler(delegate (object sender, MouseButtonEventArgs e)
					{
						Logger.Info("ContextMenu: " + file.FullName);
					});
					entry.PreviewMouseDown += new MouseButtonEventHandler(delegate (object sender, MouseButtonEventArgs e)
					{
						Boot.Core().GetWorkspace().OpenDocument(file.FullName);
					});*/

				}
			}

			return item;
		}


	}

    [ValueConversion(typeof(string), typeof(bool))]
    public class Iconizer : IValueConverter {
        public static Iconizer Instance = new Iconizer();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string name = "";

            if ((value as string).EndsWith(".lua")) {
                name = "LUA";
            } else if ((value as string).EndsWith(".tex")) {
                name = "KTEX";
            } else if ((value as string).EndsWith(".xml")) {
                name = "XML";
            } else if ((value as string).EndsWith(".zip")) {
                name = "Archive";
            } else if ((value as string).EndsWith(".png") || (value as string).EndsWith(".bmp") || (value as string).EndsWith(".gif") || (value as string).EndsWith(".jpg") || (value as string).EndsWith(".jpeg") || (value as string).EndsWith(".tif")) {
                name = "Image";
            } else if ((value as string).EndsWith(".txt")) {
                name = "Text";
            } else if ((value as string).Contains(".")) {
                name = "Unknown";
            } else {
                name = "Folder_Closed";
            }

            return new BitmapImage(new Uri("pack://application:,,,/DSTEd;component/Assets/FileSystem/" + name + ".png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            /* Do Nothing */

            return null;
        }
    }
}
