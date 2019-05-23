using System;
using System.Collections.Generic;

namespace Assets.script.jsonElements {
    [Serializable]
    public class JsonData {
        public string title;
        public List<MenuData> menus;
    }

    [Serializable]
    public class MenuData {

        public MenuData(string header, string data) {
            this.header = header;
            this.data = data;
        }

        public string header;
        public string data;
        public string unit;
        public Graph graph;
    }

    [Serializable]
    public class Graph {
        public List<float> graph;
        public string graph_start;
        public string graph_end;
    }
}
