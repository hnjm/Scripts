<Query Kind="Program" />

void Main()
{
	ToString(3981);
}

void ToString(int num){
	if(num == 0) return;	
	ToString(num / 10);
	Console.Write(num % 10);
}

/*
	process
	-------
	print digit by digit using recursive function. 
*/