using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph {

	public int nbrooms;
	public int[] tabEligible;

	/// <summary>
	/// The graph nodes.
	/// </summary>
	public List<Node> graphNodes;


	public Graph(int[] tab, int nbrooms){
		this.nbrooms = nbrooms;
		graphNodes = new List<Node> ();
		this.tabEligible = new int[nbrooms];

		for (int i = 0; i < nbrooms; i++) {
			this.tabEligible [i] = tab [i];
		}
		createGraph ();
	}


	public void createGraph(){
		int nbLine = ((int)Mathf.Sqrt (nbrooms));
		for (int i = 0; i < nbrooms; i++) {
			graphNodes.Add (new Node (i, i / nbLine, i % nbLine, int.MaxValue));
		}

		for (int i = 0; i < nbrooms; i++) {
			Node node = graphNodes [i];

			if (node.line < nbLine - 1) {
				int id = node.id + nbLine;

					node.addArc (graphNodes [id], tabEligible [id]);

			}
			if (node.line > 0) {
				int id = node.id - nbLine;
					node.addArc (graphNodes [id], tabEligible [id]);

			}
			if (node.column < nbLine - 1 ) {
				int id = node.id +1;
				//if (tabEligible [id] != int.MaxValue) {
					node.addArc (graphNodes [id], tabEligible [id]);
				
			}
			if (node.column > 0) {
				int id = node.id - 1;
				//if (tabEligible [id] != int.MaxValue) {
					node.addArc (graphNodes [id], tabEligible [id]);

			}

		}

	}


}