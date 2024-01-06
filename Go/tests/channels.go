package main

import "fmt"

func sum(s []int, cs chan int) {
	sum := 0
	for _, v := range s {
		sum += v
	}
	cs <- sum // send sum to c
}

func main() {
	s := []int{7, 2, 8, -9, 4, 0}

	c := make(chan int)
	go sum(s[:len(s)/2], c)
	go sum(s[len(s)/2:], c)
	x:= <-c
	y:= <-c

	fmt.Println(x, y, x+y)
}

