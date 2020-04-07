import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-value',
  templateUrl: './value.component.html',
  styleUrls: ['./value.component.css']
})
export class ValueComponent implements OnInit {

  /** Class property to store reponse. */
  myReceivedValues: any;

  constructor(private httpClient: HttpClient) {
  }

  /** On initialisation */
  ngOnInit() {
    this.getValues();
  }

  getValues() {
    this.httpClient.get('http://localhost:5000/api/values').subscribe(response => {
      this.myReceivedValues = response;
    }, error => {
      console.log('An error', error);
    });
  }
}
