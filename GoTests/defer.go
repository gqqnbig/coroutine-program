package main

import "fmt"

var ch1 chan int = make(chan int)
var ch2 chan bool = make(chan bool)

func s(v int) {
	defer d()
	ch1 <- v
}

func d() {
	ch2 <- true
}

func main() {
	go s(2)

	fmt.Println(<-ch1, <-ch2)
}
