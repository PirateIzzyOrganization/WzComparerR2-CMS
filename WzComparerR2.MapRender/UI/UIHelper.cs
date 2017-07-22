﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WzComparerR2.WzLib;
using WzComparerR2.Common;
using WzComparerR2.PluginBase;
using WzComparerR2.Rendering;
using EmptyKeys.UserInterface;
using EmptyKeys.UserInterface.Controls;
using EmptyKeys.UserInterface.Input;
using EmptyKeys.UserInterface.Media;
using Microsoft.Xna.Framework.Graphics;

namespace WzComparerR2.MapRender.UI
{
    static class UIHelper
    {
        public static IDisposable RegisterClickEvent<T>(UIElement control, Func<UIElement, PointF, T> getItemFunc, Action<T> onClick)
        {
            var holder = new ClickEventHolder<T>(control)
            {
                GetItemFunc = getItemFunc,
                ClickFunc = onClick,
            };
            holder.Register();
            return holder;
        }

        public static TextureBase LoadTexture(Wz_Node node)
        {
            var png = node?.GetLinkedSourceNode(PluginManager.FindWz)?.GetValueEx<Wz_Png>(null);
            return png == null ? null : Engine.Instance.Renderer.CreateTexture(png);
        }

        public static HitMap CreateHitMap(Texture2D texture)
        {
            HitMap hitMap = null;
            byte[] colorData;
            bool[] rowHit;
            switch (texture.Format)
            {
                case SurfaceFormat.Color:
                case SurfaceFormat.Bgra32:
                    hitMap = new HitMap(texture.Width, texture.Height);
                    colorData = new byte[texture.Width * texture.Height * 4];
                    rowHit = new bool[texture.Width];
                    texture.GetData(colorData);
                    for (int y = 0; y < texture.Height; y++)
                    {
                        int rowStart = y * texture.Width * 4;
                        for (int i = 0; i < rowHit.Length; i++)
                        {
                            rowHit[i] = colorData[rowStart + i * 4 + 3] != 0;
                        }
                        hitMap.SetRow(y, rowHit);
                    }
                    break;

                case SurfaceFormat.Bgra4444:
                    hitMap = new HitMap(texture.Width, texture.Height);
                    colorData = new byte[texture.Width * texture.Height * 2];
                    rowHit = new bool[texture.Width];
                    texture.GetTexture_BGRA4444(0, 0, new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height), colorData, 0, colorData.Length);
                    for (int y = 0; y < texture.Height; y++)
                    {
                        int rowStart = y * texture.Width * 2;
                        for (int i = 0; i < rowHit.Length; i++)
                        {
                            rowHit[i] = colorData[rowStart + i * 2 + 1] >> 4 != 0;
                        }
                        hitMap.SetRow(y, rowHit);
                    }
                    break;

                default:
                    hitMap = new HitMap(true);
                    break;
            }
            return hitMap;
        }

        class ClickEventHolder<T> : IDisposable
        {
            public ClickEventHolder(UIElement control)
            {
                this.Control = control;
            }

            public UIElement Control { get; private set; }
            public Func<UIElement, PointF, T> GetItemFunc { get; set; }
            public Action<T> ClickFunc { get; set; }

            private T item;

            public void Register()
            {
                this.Control.MouseDown += this.OnMouseDown;
                this.Control.MouseUp += this.OnMouseUp;
            }

            public void Deregister()
            {
                this.Control.MouseDown -= this.OnMouseDown;
                this.Control.MouseUp -= this.OnMouseUp;
            }

            private void OnMouseDown(object sender, MouseButtonEventArgs e)
            {
                if (GetItemFunc != null && e.ChangedButton == EmptyKeys.UserInterface.Input.MouseButton.Left)
                {
                    this.item = GetItemFunc.Invoke(this.Control, e.GetPosition(this.Control));
                }
            }

            private void OnMouseUp(object sender, MouseButtonEventArgs e)
            {
                if (GetItemFunc != null && e.ChangedButton == EmptyKeys.UserInterface.Input.MouseButton.Left)
                {
                    T item = GetItemFunc.Invoke(this.Control, e.GetPosition(this.Control));
                    if (item != null && object.Equals(item, this.item))
                    {
                        this.ClickFunc?.Invoke(item);
                    }
                    this.item = default(T);
                }
            }

            void IDisposable.Dispose()
            {
                this.Deregister();
            }
        }
    }
}