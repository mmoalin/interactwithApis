import React, { Component } from 'react';

export class Home extends Component {
  static displayName = Home.name;

  render () {
    return (
      <div>
        <h1>Hello, world!</h1>
        <p>Welcome to this single-page application, built with:</p>
        <ul>
          <li><a href='https://get.asp.net/'>ASP.NET Core</a> and <a href='https://msdn.microsoft.com/en-us/library/67ef8sbd.aspx'>C#</a> for cross-platform server-side code</li>
          <li><a href='https://facebook.github.io/react/'>React</a> for client-side code</li>
          <li><a href='http://getbootstrap.com/'>Bootstrap</a> for layout and styling</li>
        </ul>
        <p>Explore MusicBrainz and other APIs that offer artist data:</p>
        <ul>
          <li>Compare artists' average words within their tracks, track length etc</li>
          <li>Browse an artist searching by name and looking at their releases and tracks.</li>
          <li>Browse artists' tracks.</li>
        </ul>
        <p>Coming soon, some charts and browsing an artists' lyrics!</p>
      </div>
    );
  }
}
