using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TwoOkNotes.Model;

namespace TwoOkNotes.Util
{
    public class MaxHeap
    {

        private Dictionary<Color, int> inputDict = new();
        private List<(Color currColor, int frequency)> heap = new();

        private int Parent(int i) => (i - 1) / 2;
        private int LeftChild(int i) => 2 * i + 1;
        private int RightChild(int i) => 2 * i + 2;

        public MaxHeap(Dictionary<Color, int> inputDict)
        {
            this.inputDict = inputDict;
            HeapifyDict(inputDict);
        }

        public void Insert(Color curColor, int frequency)
        {
            heap.Add((curColor, frequency));
            HeapifyUp(heap.Count - 1);
        }

        private void HeapifyUp(int index)
        {
            while (index > 0 && heap[index].frequency > heap[Parent(index)].frequency)
            {
                (heap[index], heap[Parent(index)]) = (heap[Parent(index)], heap[index]);
                index = Parent(index);
            }
        }

        private void HeapifyDown(int index)
        {
            int maxElement = index;
            int left = LeftChild(index);
            int right = RightChild(index);

            if (left < heap.Count && heap[left].frequency > heap[maxElement].frequency)
            {
                maxElement = left;
            }

            if (right < heap.Count && heap[right].frequency > heap[maxElement].frequency)
            {
                maxElement = right;
            }

            if (maxElement != index)
            {
                (heap[index], heap[maxElement]) = (heap[maxElement], heap[index]);
                HeapifyDown(maxElement);
            }
        }

        public int ExtractMax()
        {
            if (heap.Count == 0) throw new InvalidCastException("Heap is empty");
            else
            {
                var maxValue = heap[0].frequency;
                heap[0] = heap[^1];
                heap.RemoveAt(heap.Count - 1);
                HeapifyDown(0);

            }
            return int.MaxValue;
        }

        public void HeapifyDict(Dictionary<Color, int> incHeap)
        {
            foreach (var pair in incHeap)
            {
                Insert(pair.Key, pair.Value);
            }
        }

        public Dictionary<Color, int> GetHeapyfiedDict()
        {
            Dictionary<Color, int> outputDict = new();
            foreach (var pair in heap)
            {
                outputDict.Add(pair.currColor, pair.frequency);
            }
            return outputDict;
        }
    }
}
