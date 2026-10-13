using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextAnalysisTool.NET;

namespace TextAnalysisTool.NET.Tests;

[TestClass]
public class BinaryHeapTests
{
	[TestMethod]
	public void BinaryHeap_MinHeapOrdering_PopsInAscendingOrder()
	{
		BinaryHeap<int> heap = new BinaryHeap<int>((a, b) => a.CompareTo(b));

		int[] input = new int[] { 45, 12, 89, 3, 27, 99, 1, 15, 33 };
		foreach (int val in input)
		{
			heap.Push(val);
		}

		Assert.AreEqual(input.Length, heap.Count);

		List<int> sorted = new List<int>();
		while (heap.Count > 0)
		{
			sorted.Add(heap.Pop());
		}

		int[] expected = new int[] { 1, 3, 12, 15, 27, 33, 45, 89, 99 };
		CollectionAssert.AreEqual(expected, sorted);
	}

	[TestMethod]
	public void BinaryHeap_EmptyHeap_ThrowsOnPop()
	{
		BinaryHeap<int> heap = new BinaryHeap<int>((a, b) => a.CompareTo(b));
		Assert.AreEqual(0, heap.Count);

		Assert.ThrowsException<InvalidOperationException>(() =>
		{
			heap.Pop();
		});
	}

	[TestMethod]
	public void BinaryHeap_CustomComparator_SupportsMaxHeap()
	{
		// Max heap: larger numbers first
		BinaryHeap<int> maxHeap = new BinaryHeap<int>((a, b) => b.CompareTo(a));
		maxHeap.Push(10);
		maxHeap.Push(50);
		maxHeap.Push(20);

		Assert.AreEqual(50, maxHeap.Pop());
		Assert.AreEqual(20, maxHeap.Pop());
		Assert.AreEqual(10, maxHeap.Pop());
	}
}
