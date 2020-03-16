for (($i = 20); $i -le 200; $i+=20)
{
	"$i" >> out.txt
    ./NMCP.exe $i >> out.txt
}