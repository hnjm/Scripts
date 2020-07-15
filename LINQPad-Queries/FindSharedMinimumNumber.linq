<Query Kind="Statements" />


var arr1 = new int[] { 11, 2, 5, 3, 6 };
var arr2 = new int[] { 9, 6, 3, 12, 4, 7, 1, 11, 5 };

var set = new HashSet<int>(arr1);

int? min = null;
for(var i = 0; i < arr2.Length; i++){
	if(set.Contains(arr2[i]) && (min == null || arr2[i] < min))
		min = arr2[i];
}

Console.WriteLine(min);  // minimum shared number > 3

/*
	process
	-------
	load the shorter array to hashSet, find minimum value but check the set 
	to confirm that it exists in both arrays
*/