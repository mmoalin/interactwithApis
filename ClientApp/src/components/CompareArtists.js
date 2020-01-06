import React, { Component } from 'react';

export class CompareArtists extends Component {
  static displayName = CompareArtists.name;

  constructor(props) {
    super(props);
      this.state = { ArtistName: null, stats: [], fetching: false };
      this.handleChange = this.handleChange.bind(this);
      this.populateStatsData = this.populateStatsData.bind(this);
  }
    handleChange(e) {
        this.setState({ ArtistName: e.target.value });
    }

    renderForm() {
        return (
            <div>
                <h1>Average words in an album</h1>
                <p>Get the average words in an album for a given artist</p>
                <form onSubmit={this.populateStatsData}>
                    <span>Name:</span>
                    <input type="text" name="name" onChange={this.handleChange} />
                    <input type="submit" value="Submit" />
                </form>
            </div>
        )
    }

  static renderStatsTable(forecasts) {
    return (
      <table className='table table-striped' aria-labelledby="tabelLabel">
        <thead>
          <tr>
            <th>Artist Name</th>
            <th>Average Words</th>
            <th>Variance</th>
            <th>Standard Deviation</th>
            <th>Longest Track</th>
            <th>Shortest Track</th>
          </tr>
        </thead>
        <tbody>
          {forecasts.map((row,i) =>
            <tr key={i}>
                <td>{row.artistName}</td>
                <td>{row.averageWords}</td>
                <td>{row.variance}</td>
                <td>{row.standardDeviation}</td>
                <td>{row.longestTrack}</td>
                <td>{row.shortestTrack}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }

    render() {
    let data = this.state.stats.length > 0
        ? CompareArtists.renderStatsTable(this.state.stats)
        : null;
    let contents = this.state.fetching
      ? <p><em>Loading...</em></p>
        : data;

    return (
      <div>
        <h1 id="tabelLabel" >Artist statistics</h1>
        <p>This fetches data to populate artist stats.</p>
        <form onSubmit={this.populateStatsData}>
            <span>Name:</span>
            <input type="text" name="name" onChange={this.handleChange} />
            <input type="submit" value="Submit" />
        </form>
        {contents}
      </div>
    );
  }

async populateStatsData(e) {
    e.preventDefault();
    this.setState({ fetching: true });
    const response = await fetch('ArtistStats?' + this.state.ArtistName);
    const data = await response.json();
    let current = this.state.stats;
    current.push(data);
    this.setState({ fetching: false, stats: current});
  }
}
