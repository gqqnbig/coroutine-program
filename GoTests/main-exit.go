package main

import "fmt"

func sum(s []int, cs chan int, d chan bool) {
	sum := 0
	for _, v := range s {
		sum += v
	}
	cs <- sum // send sum to c
	<-d
}

func main() {
	s := []int{7, 2, 8, -9, 4, 0}

	c := make(chan int)
	d := make(chan bool)
	go sum(s[:len(s)/2], c, d)
	go sum(s[len(s)/2:], c, d)
	x:= <-c
	y:= <-c

	fmt.Println(x, y, x+y)
}

