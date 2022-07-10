﻿using Luo_Painter.Historys;
using System.Collections.Generic;

namespace Luo_Painter.Layers
{
    public sealed class LayerDictionary : Dictionary<string, ILayer>
    {

        public void Push(ILayer layer)
        { 
            base.Add(layer.Id, layer);
        }
        public void PushChild(ILayer layer)
        {
            this.Push(layer);
            foreach (ILayer item in layer.Children)
            {
                this.PushChild(item);
            }
        }

        public IEnumerable<ILayer> Permutation(Layerage[] layerages)
        {
            if (layerages is null) yield break;

            foreach (Layerage item in layerages)
            {
                yield return this.Permutation(item, 0);
            }
        }
        private ILayer Permutation(Layerage layerage, int depth)
        {
            ILayer layer = base[layerage.Id];
            layer.Depth = depth;
            layer.Children.Clear();

            if (layerage.Children is null) return layer;

            depth++;
            foreach (Layerage item in layerage.Children)
            {
                layer.Children.Add(this.Permutation(item, depth));
            }
            return layer;
        }

    }
}